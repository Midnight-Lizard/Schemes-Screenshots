using Microsoft.Extensions.Options;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public interface IScreenshotGenerator
    {
        Task<List<Screenshot>> GenerateScreenshots(IBrowserManager browserManager, SchemePublishedEvent @event);
        Task WarmUpAsync(IBrowserManager browserManager);
        void CleanScreenshotsOutputFolder();
    }

    public class ScreenshotGenerator : IScreenshotGenerator
    {
        private readonly IOptions<BrowserConfig> browserConfig;
        private readonly IOptions<ExtensionConfig> extensionConfig;
        private readonly IOptions<ScreenshotsConfig> screenshotsConfig;

        public ScreenshotGenerator(
            IOptions<BrowserConfig> browserConfig,
            IOptions<ExtensionConfig> extensionConfig,
            IOptions<ScreenshotsConfig> screenshotsConfig)
        {
            this.browserConfig = browserConfig;
            this.extensionConfig = extensionConfig;
            this.screenshotsConfig = screenshotsConfig;
        }

        public async Task<List<Screenshot>> GenerateScreenshots(
            IBrowserManager browserManager, SchemePublishedEvent @event)
        {
            using (browserManager)
            {
                var results = new List<Screenshot>();
                var config = this.screenshotsConfig.Value;
                await browserManager.LaunchAsync(this.browserConfig.Value, this.extensionConfig.Value);
                var colorSchemeNameEncoded = WebUtility.UrlEncode(@event.ColorScheme.colorSchemeName);

                foreach (var (url, title) in from url in config.SCREENSHOT_URLS.Split(',', '~', StringSplitOptions.RemoveEmptyEntries)
                                                .Select((url, i) => (url, i))
                                             join title in config.SCREENSHOT_URL_TITLES.Split(',', '~', StringSplitOptions.RemoveEmptyEntries)
                                                 .Select((title, i) => (title, i))
                                                 on url.i equals title.i
                                             select (url.url, title.title))
                {
                    //var urlWithName = url.Replace($"{{{nameof(ColorScheme.colorSchemeName)}}}", colorSchemeNameEncoded);
                    foreach (var size in config.SCREENSHOT_SIZES.Split(',', '~', StringSplitOptions.RemoveEmptyEntries)
                        .Select(sizeStr => new ScreenshotSize(sizeStr)))
                    {
                        // TODO: remove urlWithName assignment below and uncomment above
                        var urlWithName = url.Replace($"{{{nameof(ColorScheme.colorSchemeName)}}}", colorSchemeNameEncoded + "+" + size.Width.ToString());
                        var outFilePath = Path.Combine(config.SCREENSHOT_OUT_DIR, Guid.NewGuid().ToString() + ".png");
                        await browserManager.ScreenshotAsync(urlWithName, size, outFilePath);
                        results.Add(new Screenshot
                        {
                            AggregateId = @event.AggregateId,
                            Url = urlWithName,
                            Title = title,
                            Size = size,
                            FilePath = outFilePath
                        });
                    }
                }
                return results;
            }
        }

        public async Task WarmUpAsync(IBrowserManager browserManager)
        {
            using (browserManager)
            {
                var results = new List<Screenshot>();
                var config = this.screenshotsConfig.Value;
                await browserManager.LaunchAsync(this.browserConfig.Value, this.extensionConfig.Value);
                foreach (var url in config.SCREENSHOT_URLS.Split(',', '~', StringSplitOptions.RemoveEmptyEntries))
                {
                    foreach (var size in config.SCREENSHOT_SIZES.Split(',', '~', StringSplitOptions.RemoveEmptyEntries)
                        .Select(sizeStr => new ScreenshotSize(sizeStr)))
                    {
                        await browserManager.WarmUpAsync(url, size);
                    }
                }
            }
        }

        public void CleanScreenshotsOutputFolder()
        {
            var outDir = this.screenshotsConfig.Value.SCREENSHOT_OUT_DIR;
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }
            Directory.CreateDirectory(outDir);
        }
    }
}
