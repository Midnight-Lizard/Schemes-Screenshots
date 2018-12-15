using FluentAssertions;
using Microsoft.Extensions.Options;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Testing.Utilities;
using NSubstitute;
using System;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public class ScreenshotGeneratorSpec
    {
        private readonly ScreenshotGenerator generator;
        private readonly IProgressiveImageConverter progressiveImageConverterStub = Substitute.For<IProgressiveImageConverter>();
        private readonly IBrowserManager browserManager = Substitute.For<IBrowserManager>();
        private readonly BrowserConfig browserConfig = new BrowserConfig();
        private readonly ExtensionConfig extensionConfig = new ExtensionConfig();
        private readonly ScreenshotsConfig screenshotsConfig = new ScreenshotsConfig
        {
            SCREENSHOT_URLS =
                    "https://www.google.com/search?hl=en&q={colorSchemeName}," +
                    "https://www.google.com/search?hl=en&tbm=isch&q={colorSchemeName},",
            SCREENSHOT_SIZES = "xl:1280x800x200,md:960x600x200,xs:640x400x200,",
            SCREENSHOT_OUT_DIR = "./img",
            SCREENSHOT_URL_TITLES = "Google Search,Google Search Images,"
        };
        private readonly SchemePublishedEvent publishedEvent = new SchemePublishedEvent
        {
            Id = "agg-id",
            ColorScheme = new ColorScheme
            {
                colorSchemeId = "cs-id",
                colorSchemeName = "cs-test/name"
            }
        };

        public ScreenshotGeneratorSpec()
        {
            var browserConfigOptions = Substitute.For<IOptions<BrowserConfig>>();
            var extConfigOptions = Substitute.For<IOptions<ExtensionConfig>>();
            var screenshotsConfigOptions = Substitute.For<IOptions<ScreenshotsConfig>>();

            browserConfigOptions.Value.Returns(this.browserConfig);
            extConfigOptions.Value.Returns(this.extensionConfig);
            screenshotsConfigOptions.Value.Returns(this.screenshotsConfig);

            this.generator = new ScreenshotGenerator(this.progressiveImageConverterStub,
                browserConfigOptions, extConfigOptions, screenshotsConfigOptions);
        }

        public class GenerateScreenshotsSpec : ScreenshotGeneratorSpec
        {
            [It(nameof(ScreenshotGenerator.GenerateScreenshots))]
            public async void Should_launch_browser()
            {
                await this.generator.GenerateScreenshots(this.browserManager, this.publishedEvent);

                await this.browserManager.Received(1).LaunchAsync(this.browserConfig, this.extensionConfig);
            }

            [It(nameof(ScreenshotGenerator.GenerateScreenshots))]
            public async void Should_dispose_browser_at_the_end()
            {
                await this.generator.GenerateScreenshots(this.browserManager, this.publishedEvent);

                this.browserManager.Received(1).Dispose();
            }

            [It(nameof(ScreenshotGenerator.GenerateScreenshots))]
            public async void Should_return_screenshots_for_all_urls_and_sizes()
            {
                var results = await this.generator.GenerateScreenshots(this.browserManager, this.publishedEvent);

                var totalShots = this.screenshotsConfig.SCREENSHOT_SIZES.Split(',', StringSplitOptions.RemoveEmptyEntries).Length *
                    this.screenshotsConfig.SCREENSHOT_URLS.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;

                results.Should().HaveCount(totalShots);
            }

            [It(nameof(ScreenshotGenerator.GenerateScreenshots))]
            public async void Should_convert_all_screenshots_into_Progressive_JPEG_format()
            {
                await this.generator.GenerateScreenshots(this.browserManager, this.publishedEvent);

                var totalShots = this.screenshotsConfig.SCREENSHOT_SIZES.Split(',', StringSplitOptions.RemoveEmptyEntries).Length *
                    this.screenshotsConfig.SCREENSHOT_URLS.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;

                this.progressiveImageConverterStub.Received(totalShots)
                    .ConvertPngToProgressiveJpeg(Arg.Any<string>(), Arg.Any<string>(),
                        Arg.Any<ProgressiveImageConverterOptions>());
            }

            [It(nameof(ScreenshotGenerator.GenerateScreenshots))]
            public async void Should_return_screenshots_for_the_specified_aggregate()
            {
                var results = await this.generator.GenerateScreenshots(this.browserManager, this.publishedEvent);

                results.Should().OnlyContain(x => x.PublicSchemeId == this.publishedEvent.Id);
            }
        }
    }
}
