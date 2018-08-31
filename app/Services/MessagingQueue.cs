using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.EventHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public enum QueueStatus
    {
        Stopped = 0,
        Running = 1,
        Paused = 2
    }

    public interface IMessagingQueue
    {
        Task BeginProcessing(CancellationToken token);
        bool CheckStatus();
        Task PauseProcessing();
        Task ResumeProcessing(CancellationToken token);
    }

    public class MessagingQueue : IMessagingQueue
    {
        protected QueueStatus queueStatus = QueueStatus.Stopped;
        protected Message<string, string> lastConsumedEvent;
        protected Message<string, string> lastConsumedRequest;
        protected readonly TimeSpan timeout = TimeSpan.FromSeconds(1);
        protected List<TopicPartition> assignedEventsPartitions;
        protected readonly ILogger<MessagingQueue> logger;
        protected readonly KafkaConfig kafkaConfig;
        private readonly BaseSchemeEventHandler schemeEventHandler;
        protected Consumer<string, string> eventsConsumer;
        protected CancellationToken cancellationToken;
        protected TaskCompletionSource<bool> queuePausingCompleted;
        private readonly bool thereMayBeNewMessages;
        private int errorCount = 0;
        private readonly int maxErrorCount = 10;

        public MessagingQueue(
            ILogger<MessagingQueue> logger,
            KafkaConfig config,
            BaseSchemeEventHandler schemeEventHandler)
        {
            this.logger = logger;
            this.kafkaConfig = config;
            this.schemeEventHandler = schemeEventHandler;
        }

        public bool CheckStatus()
        {
            if (this.queueStatus == QueueStatus.Running || this.errorCount < this.maxErrorCount)
            {
                switch (this.queueStatus)
                {
                    case QueueStatus.Paused:
                        this.ResumeProcessing(this.cancellationToken);
                        break;
                    case QueueStatus.Stopped:
                        this.BeginProcessing(this.cancellationToken);
                        break;
                }
                return true;
            }
            return false;
        }

        public async Task PauseProcessing()
        {
            this.queuePausingCompleted = new TaskCompletionSource<bool>();
            this.queueStatus = QueueStatus.Paused;
            await this.queuePausingCompleted.Task;
        }

        public async Task ResumeProcessing(CancellationToken token)
        {
            this.queueStatus = QueueStatus.Stopped;
            await this.BeginProcessing(token);
        }

        public async Task BeginProcessing(CancellationToken token)
        {
            if (this.cancellationToken != token)
            {
                this.cancellationToken = token;
                token.Register(async () =>
                {
                    while (this.queueStatus == QueueStatus.Running)
                    {
                        await Task.Delay(this.timeout);
                    }
                });
            }

            if (this.queueStatus == QueueStatus.Stopped)
            {
                try
                {
                    await schemeEventHandler.Init();
                    this.queueStatus = QueueStatus.Running;
                    using (var eventsConsumer = new Consumer<string, string>(
                        this.kafkaConfig.KAFKA_EVENTS_CONSUMER_CONFIG,
                        new StringDeserializer(Encoding.UTF8),
                        new StringDeserializer(Encoding.UTF8)))
                    {
                        this.eventsConsumer = eventsConsumer;

                        this.eventsConsumer.Subscribe(this.kafkaConfig.SCHEMES_EVENTS_TOPIC);

                        while (this.queueStatus == QueueStatus.Running && !this.cancellationToken.IsCancellationRequested)
                        {
                            if (this.eventsConsumer.Consume(out var @event, this.timeout))
                            {
                                await this.HandleMessage(@event);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to consume new messages");
                    this.errorCount++;
                }
                finally
                {
                    switch (this.queueStatus)
                    {
                        case QueueStatus.Paused:
                            this.queuePausingCompleted?.SetResult(true);
                            break;
                        case QueueStatus.Running:
                        case QueueStatus.Stopped:
                        default:
                            this.queueStatus = QueueStatus.Stopped;
                            break;
                    }
                }
            }
        }

        protected async Task HandleMessage(Message<string, string> kafkaMessage)
        {
            if (kafkaMessage.Error.HasError)
            {
                this.logger.LogError($"Failed to consume [{kafkaMessage.Value ?? "message"}] with reason: {kafkaMessage.Error.Reason}");
            }
            else
            {
                await this.schemeEventHandler.HandleTransportEvent(kafkaMessage.Value);
            }
        }
    }
}