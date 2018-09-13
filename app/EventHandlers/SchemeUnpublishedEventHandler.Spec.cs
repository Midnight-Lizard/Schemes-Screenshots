using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Schemes.Screenshots.Services;
using MidnightLizard.Testing.Utilities;
using Newtonsoft.Json;
using NSubstitute;
using System;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public class SchemeUnpublishedEventHandlerSpec
    {
        private readonly ILogger<SchemeUnpublishedEventHandler> loggerStub;
        private readonly IScreenshotUploader screenshotUploaderStub;
        private readonly SchemeUnpublishedEventHandler handler;

        public SchemeUnpublishedEventHandlerSpec()
        {
            this.loggerStub = Substitute.For<ILogger<SchemeUnpublishedEventHandler>>();
            this.screenshotUploaderStub = Substitute.For<IScreenshotUploader>();
            this.handler = new SchemeUnpublishedEventHandler(this.loggerStub, this.screenshotUploaderStub);

        }

        public class HandleEventSpec : SchemeUnpublishedEventHandlerSpec
        {
            private readonly string testEventJson;
            private readonly SchemeUnpublishedEvent testEvent = new SchemeUnpublishedEvent
            {
                AggregateId = "agg-test-id"
            };
            public HandleEventSpec()
            {
                this.testEventJson = JsonConvert.SerializeObject(this.testEvent);
            }

            [It(nameof(SchemeUnpublishedEventHandler.HandleEvent))]
            public void Should_delete_screenshots()
            {
                this.handler.HandleEvent(this.testEventJson);

                this.screenshotUploaderStub.Received(1).DeleteScrenshots(this.testEvent.AggregateId);
            }

            [It(nameof(SchemePublishedEventHandler.HandleEvent))]
            public void Should_log_error_when_failed_to_delete_screenshots_from_CDN()
            {
                var testException = new Exception("test");

                this.screenshotUploaderStub
                    .When(x => x.DeleteScrenshots(this.testEvent.AggregateId))
                    .Throw(testException);

                this.handler.HandleEvent(this.testEventJson);

                this.loggerStub.Received(1)
                    .Log(LogLevel.Error, 0, Arg.Any<FormattedLogValues>(), testException, Arg.Any<Func<object, Exception, string>>());
            }
        }
    }
}
