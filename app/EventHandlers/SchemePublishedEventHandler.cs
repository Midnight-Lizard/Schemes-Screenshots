using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Schemes.Screenshots.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public interface ISchemePublishedEventHandler
    {
        Task Init();
        Task HandleEvent(string schemePublishedEventJsonString);
    }

    public class SchemePublishedEventHandler : ISchemePublishedEventHandler
    {
        private readonly ILogger<SchemePublishedEventHandler> logger;
        private readonly IExtensionManager extensionManager;
        private readonly IScreenshotGenerator screenshotGenerator;
        private readonly IScreenshotUploader screenshotUploader;

        public SchemePublishedEventHandler(
            ILogger<SchemePublishedEventHandler> logger,
            IExtensionManager extensionManager,
            IScreenshotGenerator screenshotGenerator,
            IScreenshotUploader screenshotUploader)
        {
            this.logger = logger;
            this.extensionManager = extensionManager;
            this.screenshotGenerator = screenshotGenerator;
            this.screenshotUploader = screenshotUploader;
        }

        public async Task Init()
        {
            await this.extensionManager.DownloadExtension();
            logger.LogInformation("Extension downloaded.");
            this.extensionManager.ExtractExtension();
            logger.LogInformation("Extension extracted.");
            await this.screenshotGenerator.WarmUpAsync(new BrowserManager());
            logger.LogInformation("Browser warmed up.");
        }

        public async Task HandleEvent(string schemePublishedEventJsonString)
        {
            this.extensionManager.ExtractExtension();
            await this.extensionManager.ReplaceDefaultColorScheme(
                JObject.Parse(schemePublishedEventJsonString)[nameof(SchemePublishedEvent.ColorScheme)]);

            this.screenshotGenerator.CleanScreenshotsOutputFolder();

            var results = await this.screenshotGenerator.GenerateScreenshots(
                new BrowserManager(),
                JsonConvert.DeserializeObject<SchemePublishedEvent>(schemePublishedEventJsonString));

            foreach (var shot in results)
            {
                try
                {
                    this.screenshotUploader.UploadScreenshot(shot);
                }
                catch (System.Exception ex)
                {
                    this.logger.LogError(ex, $"Failed to upload screenshot to CDN for PublicScheme [{shot.PublicSchemeId}]");
                }
            }
        }
    }
}
