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
        Task HandleTransportEvent(string transportEventJsonString);
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

        public async Task HandleTransportEvent(string transportEventJsonString)
        {
            try
            {
                var transEvent = JsonConvert.DeserializeObject<TransportEvent>(transportEventJsonString);
                switch (transEvent.Type)
                {
                    case nameof(SchemePublishedEvent):
                    {
                        await this.schemePublishedEventHandler.HandleEvent(transEvent.Payload.Value as string);
                        break;
                    }

                    case nameof(SchemeUnpublishedEvent):
                    {
                        this.schemeUnpublishedEventHandler.HandleEvent(transEvent.Payload.Value as string);
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
                this.logger.LogError(ex, $"Failed to handle [{transportEventJsonString ?? "event"}]");
            }
        }
    }
}
