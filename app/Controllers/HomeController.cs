using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Schemes.Screenshots.Services;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IExtensionManager extensionManager;
        private readonly IScreenshotGenerator screenshotGenerator;
        private readonly IScreenshotUploader screenshotUploader;
        private readonly IOptions<BrowserConfig> browserConfig;
        private readonly IOptions<ExtensionConfig> extensionConfig;

        public HomeController(
            IExtensionManager extensionManager,
            IScreenshotGenerator screenshotGenerator,
            IScreenshotUploader screenshotUploader,
            IOptions<BrowserConfig> browserConfig,
            IOptions<ExtensionConfig> extensionConfig)
        {
            this.extensionManager = extensionManager;
            this.screenshotGenerator = screenshotGenerator;
            this.screenshotUploader = screenshotUploader;
            this.browserConfig = browserConfig;
            this.extensionConfig = extensionConfig;
        }

        public async Task<ActionResult> WarmUp()
        {
            await this.extensionManager.DownloadExtension();
            this.extensionManager.ExtractExtension();
            await this.screenshotGenerator.WarmUpAsync(new BrowserManager());
            return Ok();
        }

        public async Task<ActionResult> Generate()
        {
            await this.extensionManager.DownloadExtension();
            this.extensionManager.ExtractExtension();
            await this.extensionManager.ReplaceDefaultColorScheme(JObject.Parse(this.newColorScheme));

            this.screenshotGenerator.CleanScreenshotsOutputFolder();

            var results = await this.screenshotGenerator.GenerateScreenshots(
                new BrowserManager(),
                new SchemePublishedEvent
                {
                    Id = "agg-test-id",
                    ColorScheme = new ColorScheme
                    {
                        colorSchemeId = "cs-test-id",
                        colorSchemeName = "cs-test-name"
                    }
                });

            //foreach (var shot in results)
            //{
            //    screenshotUploader.UploadScreenshot(shot);
            //}

            return this.Content(string.Join("<br/>", results
                .Select(x => $"<a style=\"font-size:20px\" href=\"{x.FilePath.Replace("./wwwroot", "")}\">{x.Url} -- {x.Size.Width}x{x.Size.Height}</a>")), "text/html");
        }

        private readonly string newColorScheme = @"{
            ""colorSchemeId"": ""dimmedDust"",
            ""colorSchemeName"": ""Dimmed Dust"",
            ""runOnThisSite"": true,
            ""restoreColorsOnCopy"": false,
            ""restoreColorsOnPrint"": true,
            ""doNotInvertContent"": false,
            ""blueFilter"": 5,
            ""mode"": ""auto"",
            ""modeAutoSwitchLimit"": 5000,
            ""useDefaultSchedule"": true,
            ""scheduleStartHour"": 0,
            ""scheduleFinishHour"": 24,
            ""includeMatches"": """",
            ""excludeMatches"": """",
            ""backgroundSaturationLimit"": 60,
            ""backgroundContrast"": 50,
            ""backgroundLightnessLimit"": 11,
            ""backgroundGraySaturation"": 30,
            ""backgroundGrayHue"": 36,
            ""backgroundReplaceAllHues"": false,
            ""backgroundHueGravity"": 80,
            ""buttonSaturationLimit"": 60,
            ""buttonContrast"": 3,
            ""buttonLightnessLimit"": 13,
            ""buttonGraySaturation"": 50,
            ""buttonGrayHue"": 18,
            ""buttonReplaceAllHues"": false,
            ""buttonHueGravity"": 80,
            ""textSaturationLimit"": 60,
            ""textContrast"": 64,
            ""textLightnessLimit"": 85,
            ""textGraySaturation"": 10,
            ""textGrayHue"": 90,
            ""textSelectionHue"": 32,
            ""textReplaceAllHues"": false,
            ""textHueGravity"": 80,
            ""linkSaturationLimit"": 60,
            ""linkContrast"": 50,
            ""linkLightnessLimit"": 70,
            ""linkDefaultSaturation"": 60,
            ""linkDefaultHue"": 88,
            ""linkVisitedHue"": 36,
            ""linkReplaceAllHues"": false,
            ""linkHueGravity"": 80,
            ""borderSaturationLimit"": 60,
            ""borderContrast"": 30,
            ""borderLightnessLimit"": 50,
            ""borderGraySaturation"": 20,
            ""borderGrayHue"": 60,
            ""borderReplaceAllHues"": false,
            ""borderHueGravity"": 80,
            ""imageLightnessLimit"": 80,
            ""imageSaturationLimit"": 90,
            ""useImageHoverAnimation"": false,
            ""backgroundImageLightnessLimit"": 40,
            ""backgroundImageSaturationLimit"": 80,
            ""scrollbarSaturationLimit"": 20,
            ""scrollbarContrast"": 0,
            ""scrollbarLightnessLimit"": 40,
            ""scrollbarGrayHue"": 45,
            ""scrollbarSize"": 10,
            ""scrollbarStyle"": ""true""
}";
    }
}
