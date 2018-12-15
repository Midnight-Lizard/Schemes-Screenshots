using Microsoft.Extensions.Logging;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Testing.Utilities;
using Newtonsoft.Json;
using NSubstitute;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public class RootSchemeEventHandlerSpec
    {
        private readonly ISchemePublishedEventHandler schemePublishedEventHandler;
        private readonly ISchemeUnpublishedEventHandler schemeUnpublishedEventHandler;
        private readonly RootSchemeEventHandler handler;

        public RootSchemeEventHandlerSpec()
        {
            var loggerStub = Substitute.For<ILogger<RootSchemeEventHandler>>();
            this.schemePublishedEventHandler = Substitute.For<ISchemePublishedEventHandler>();
            this.schemeUnpublishedEventHandler = Substitute.For<ISchemeUnpublishedEventHandler>();
            this.handler = new RootSchemeEventHandler(loggerStub,
                this.schemePublishedEventHandler, this.schemeUnpublishedEventHandler);
        }

        [It(nameof(RootSchemeEventHandler) + "." + nameof(RootSchemeEventHandler.Init))]
        public async void Should_init_underlining_handlers()
        {
            await this.handler.Init();

            await this.schemePublishedEventHandler.Received(1).Init();
        }

        [It(nameof(RootSchemeEventHandler) + "." + nameof(RootSchemeEventHandler.HandleTransportEvent))]
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

        [It(nameof(RootSchemeEventHandler) + "." + nameof(RootSchemeEventHandler.HandleTransportEvent))]
        public async void Should_use_SchemeUnpublishedEventHandler_when_event_has_its_type()
        {
            var payload = "\"data\"";
            var transEvent = new TransportEvent
            {
                Type = nameof(SchemeUnpublishedEvent),
                Payload = new Newtonsoft.Json.Linq.JRaw(payload)
            };

            await this.handler.HandleTransportEvent(JsonConvert.SerializeObject(transEvent));

            this.schemeUnpublishedEventHandler.Received(1).HandleEvent(payload);
        }
    }
}
