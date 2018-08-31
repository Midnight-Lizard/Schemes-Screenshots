using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Schemes.Screenshots.Services;
using MidnightLizard.Testing.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public class SchemePublishedEventHandlerSpec
    {
        private readonly ILogger<SchemePublishedEventHandler> loggerStub;
        private readonly IExtensionManager extensionManagerStub;
        private readonly IScreenshotGenerator screenshotGeneratorStub;
        private readonly IScreenshotUploader screenshotUploaderStub;
        private readonly SchemePublishedEventHandler handler;

        public SchemePublishedEventHandlerSpec()
        {
            this.loggerStub = Substitute.For<ILogger<SchemePublishedEventHandler>>();
            this.extensionManagerStub = Substitute.For<IExtensionManager>();
            this.screenshotGeneratorStub = Substitute.For<IScreenshotGenerator>();
            this.screenshotUploaderStub = Substitute.For<IScreenshotUploader>();

            this.handler = new SchemePublishedEventHandler(this.loggerStub,
                this.extensionManagerStub, this.screenshotGeneratorStub, this.screenshotUploaderStub);
        }

        public class InitSpec : SchemePublishedEventHandlerSpec
        {
            [It(nameof(SchemePublishedEventHandler.Init))]
            public async void Should_download_extension()
            {
                await this.handler.Init();

                await this.extensionManagerStub.Received(1).DownloadExtension();
            }

            [It(nameof(SchemePublishedEventHandler.Init))]
            public async void Should_extract_extension()
            {
                await this.handler.Init();

                this.extensionManagerStub.Received(1).ExtractExtension();
            }

            [It(nameof(SchemePublishedEventHandler.Init))]
            public async void Should_warm_screenshot_generator_up()
            {
                await this.handler.Init();

                await this.screenshotGeneratorStub.Received(1).WarmUpAsync(Arg.Any<IBrowserManager>());
            }
        }

        public class HandleEventSpec : SchemePublishedEventHandlerSpec
        {
            private readonly string testEventJson;
            private readonly SchemePublishedEvent testEvent = new SchemePublishedEvent
            {
                AggregateId = "agg-test-id",
                ColorScheme = new ColorScheme
                {
                    colorSchemeId = "cs-test-id",
                    colorSchemeName = "cs-test-name"
                }
            };
            private readonly List<Screenshot> screenshots = new List<Screenshot>
            {
                new Screenshot { AggregateId="1" },
                new Screenshot { AggregateId="2" },
                new Screenshot { AggregateId="3" }
            };

            public HandleEventSpec()
            {
                this.testEventJson = JsonConvert.SerializeObject(this.testEvent);
                this.screenshotGeneratorStub
                    .GenerateScreenshots(Arg.Any<IBrowserManager>(), Arg.Any<SchemePublishedEvent>())
                    .Returns(this.screenshots);
            }

            [It(nameof(SchemePublishedEventHandler.HandleEvent))]
            public async void Should_extract_extension()
            {
                await this.handler.HandleEvent(this.testEventJson);

                this.extensionManagerStub.Received(1).ExtractExtension();
            }

            [It(nameof(SchemePublishedEventHandler.HandleEvent))]
            public async void Should_replace_default_color_scheme()
            {
                await this.handler.HandleEvent(this.testEventJson);

                await this.extensionManagerStub.Received(1)
                    .ReplaceDefaultColorScheme(Arg.Is<JToken>(x =>
                    x[nameof(ColorScheme.colorSchemeId)].Value<string>() ==
                    this.testEvent.ColorScheme.colorSchemeId));
            }

            [It(nameof(SchemePublishedEventHandler.HandleEvent))]
            public async void Should_clean_screenshots_output_folder()
            {
                await this.handler.HandleEvent(this.testEventJson);

                this.screenshotGeneratorStub.Received(1).CleanScreenshotsOutputFolder();
            }

            [It(nameof(SchemePublishedEventHandler.HandleEvent))]
            public async void Should_generate_screenshots()
            {
                await this.handler.HandleEvent(this.testEventJson);

                await this.screenshotGeneratorStub.Received(1).GenerateScreenshots(
                    Arg.Any<IBrowserManager>(),
                    Arg.Is<SchemePublishedEvent>(x =>
                        x.AggregateId == this.testEvent.AggregateId &&
                        x.ColorScheme.colorSchemeId == this.testEvent.ColorScheme.colorSchemeId));
            }

            [It(nameof(SchemePublishedEventHandler.HandleEvent))]
            public async void Should_upload_all_screenshots_to_CDN()
            {
                await this.handler.HandleEvent(this.testEventJson);

                this.screenshotUploaderStub.Received(this.screenshots.Count)
                    .UploadScreenshot(Arg.Is<Screenshot>(x => this.screenshots.Contains(x)));
            }

            [It(nameof(SchemePublishedEventHandler.HandleEvent))]
            public async void Should_log_error_when_failed_to_upload_screenshot_to_CDN()
            {
                var testException = new Exception("test");

                this.screenshotUploaderStub
                    .When(x => x.UploadScreenshot(Arg.Any<Screenshot>()))
                    .Throw(testException);

                await this.handler.HandleEvent(this.testEventJson);

                this.loggerStub.Received(this.screenshots.Count)
                    .Log(LogLevel.Error, 0, Arg.Any<FormattedLogValues>(), testException, Arg.Any<Func<object, Exception, string>>());
            }
        }
    }
}
