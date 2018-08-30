using Microsoft.Extensions.Options;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public interface IExtensionManager
    {
        Task DownloadExtension();
        void ExtractExtension();
        Task ReplaceDefaultColorScheme(JObject newColorSchemeJson);
    }

    public class ExtensionManager : IExtensionManager
    {
        private readonly IOptions<ExtensionConfig> extensionConfig;

        public ExtensionManager(IOptions<ExtensionConfig> extensionConfig)
        {
            this.extensionConfig = extensionConfig;
        }

        public async Task DownloadExtension()
        {
            using (var client = new WebClient())
            {
                var config = this.extensionConfig.Value;
                new FileInfo(config.EXTENSION_ARCHIVE_PATH).Directory.Create();
                await client.DownloadFileTaskAsync(
                    config.EXTENSION_DOWNLOAD_URL,
                    config.EXTENSION_ARCHIVE_PATH);
            }
        }

        public void ExtractExtension()
        {
            var config = this.extensionConfig.Value;
            if (Directory.Exists(config.EXTENSION_EXTRACT_PATH))
            {
                Directory.Delete(config.EXTENSION_EXTRACT_PATH, true);
            }
            Directory.CreateDirectory(config.EXTENSION_EXTRACT_PATH);
            ZipFile.ExtractToDirectory(
                config.EXTENSION_ARCHIVE_PATH,
                config.EXTENSION_EXTRACT_PATH,
                true);
        }

        public async Task ReplaceDefaultColorScheme(JObject newColorSchemeJson)
        {
            var config = this.extensionConfig.Value;
            var contentScriptFilePath = Path.Combine(config.EXTENSION_EXTRACT_PATH, "./js/content-script.js");
            var contentScript = await System.IO.File.ReadAllTextAsync(contentScriptFilePath);

            newColorSchemeJson[nameof(ColorScheme.colorSchemeId)] = config.EXTENSION_DEFAULT_COLOR_SCHEME_ID;

            var newContentScript = Regex.Replace(contentScript,
                $"colorSchemeId: \"{config.EXTENSION_DEFAULT_COLOR_SCHEME_ID}\",[^}}]+",
                newColorSchemeJson.ToString().Trim('{', '}'));

            await System.IO.File.WriteAllTextAsync(contentScriptFilePath, newContentScript);
        }
    }
}
