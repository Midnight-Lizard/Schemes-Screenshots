using Microsoft.Extensions.Logging;
using MidnightLizard.Schemes.Screenshots.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public interface IBaseSchemeEventHandler
    {
        Task Init();
        Task HandleTransportEvent(string transportEventJsonString);
    }

    public class BaseSchemeEventHandler : IBaseSchemeEventHandler
    {
        private readonly ILogger<BaseSchemeEventHandler> logger;
        private readonly ISchemePublishedEventHandler schemePublishedEventHandler;

        public BaseSchemeEventHandler(
            ILogger<BaseSchemeEventHandler> logger,
            ISchemePublishedEventHandler schemePublishedEventHandler)
        {
            this.logger = logger;
            this.schemePublishedEventHandler = schemePublishedEventHandler;
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
