using Microsoft.Extensions.Logging;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Testing.Utilities;
using Newtonsoft.Json;
using NSubstitute;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public class BaseSchemeEventHandlerSpec
    {
        private readonly ISchemePublishedEventHandler schemePublishedEventHandler;
        private readonly BaseSchemeEventHandler handler;

        public BaseSchemeEventHandlerSpec()
        {
            var loggerStub = Substitute.For<ILogger<BaseSchemeEventHandler>>();
            this.schemePublishedEventHandler = Substitute.For<ISchemePublishedEventHandler>();
            this.handler = new BaseSchemeEventHandler(loggerStub, this.schemePublishedEventHandler);
        }

        [It(nameof(BaseSchemeEventHandler) + "." + nameof(BaseSchemeEventHandler.Init))]
        public async void Should_init_underlining_handlers()
        {
            await this.handler.Init();

            await this.schemePublishedEventHandler.Received(1).Init();
        }

        [It(nameof(BaseSchemeEventHandler) + "." + nameof(BaseSchemeEventHandler.HandleTransportEvent))]
        public async void Should_use_SchemePublishedEventHandler_when_event_has_its_type()
        {
            var payload = "\"data\"";
            var transEvent = new TransportEvent
            {
                Type = nameof(SchemePublishedEvent),
                Payload = new Newtonsoft.Json.Linq.JRaw(payload)
            };

            await this.handler.HandleTransportEvent(JsonConvert.SerializeObject(transEvent));

            await this.schemePublishedEventHandler.Received(1).HandleEvent(payload);
        }
    }
}
