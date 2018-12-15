using Microsoft.Extensions.Logging;
using MidnightLizard.Schemes.Screenshots.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public interface IRootSchemeEventHandler
    {
        Task Init();
        Task HandleTransportEvent(string eventJsonString);
    }

    public class RootSchemeEventHandler : IRootSchemeEventHandler
    {
        private readonly ILogger<RootSchemeEventHandler> logger;
        private readonly ISchemePublishedEventHandler schemePublishedEventHandler;
        private readonly ISchemeUnpublishedEventHandler schemeUnpublishedEventHandler;

        public RootSchemeEventHandler(
            ILogger<RootSchemeEventHandler> logger,
            ISchemePublishedEventHandler schemePublishedEventHandler,
            ISchemeUnpublishedEventHandler schemeUnpublishedEventHandler)
        {
            this.logger = logger;
            this.schemePublishedEventHandler = schemePublishedEventHandler;
            this.schemeUnpublishedEventHandler = schemeUnpublishedEventHandler;
        }

        public async Task Init()
        {
            await this.schemePublishedEventHandler.Init();
        }

        public async Task HandleTransportEvent(string eventJsonString)
        {
            try
            {
                var transEvent = JsonConvert.DeserializeObject<TransportEvent>(eventJsonString);
                switch (transEvent.Type)
                {
                    case nameof(SchemePublishedEvent):
                    {
                        await this.schemePublishedEventHandler.HandleEvent(eventJsonString);
                        break;
                    }

                    case nameof(SchemeUnpublishedEvent):
                    {
                        this.schemeUnpublishedEventHandler.HandleEvent(eventJsonString);
                        break;
                    }

                    default:
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to handle [{eventJsonString ?? "event"}]");
            }
        }
    }
}
