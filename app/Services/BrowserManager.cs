using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Models;
using PuppeteerSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public interface IBrowserManager : IDisposable
    {
        Task ScreenshotAsync(string url, ScreenshotSize viewportSize, string screenshotFilePath);
        Task LaunchAsync(BrowserConfig browserConfig, ExtensionConfig extensionConfig);
        Task WarmUpAsync(string url, ScreenshotSize size);
    }

    public class BrowserManager : IBrowserManager
    {
        private Browser browser;

        public async Task LaunchAsync(BrowserConfig browserConfig, ExtensionConfig extensionConfig)
        {
            var processName = browserConfig.CHROME_KILL_EXISTING_PROCESSES;
            if (!string.IsNullOrEmpty(processName))
            {
                foreach (var chrome in Process.GetProcessesByName(processName))
                {
                    chrome.Kill();
                }
            }
            var extensionFullPath = Path.GetFullPath(extensionConfig.EXTENSION_EXTRACT_PATH);
            this.browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = browserConfig.CHROME_EXECUTABLE_PATH,
                Headless = false,
                Args = new[] {
                    $"--load-extension={extensionFullPath}",
                    $"--disable-extensions-except={extensionFullPath}"
                }
                .Union(browserConfig.CHROME_FLAGS.Split(
                    new[] { ",", "~" }, System.StringSplitOptions.RemoveEmptyEntries))
                .ToArray()
            });
        }

        public async Task ScreenshotAsync(string url, ScreenshotSize size, string outFilePath)
        {
            using (var page = await this.browser.NewPageAsync())
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = size.Width,
                    Height = size.Height,
                    DeviceScaleFactor = size.Scale / 100.0
                });
                await page.GoToAsync(url, 10000, new[] { WaitUntilNavigation.Load });
                await Task.Delay(1000);
                await page.ScreenshotAsync(outFilePath, new ScreenshotOptions()
                {
                    Type = ScreenshotType.Png
                });
            }
        }

        public async Task WarmUpAsync(string url, ScreenshotSize size)
        {
            using (var page = await this.browser.NewPageAsync())
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = size.Width,
                    Height = size.Height,
                    DeviceScaleFactor = size.Scale / 100.0
                });
                await page.GoToAsync(url, 60000, new[] { WaitUntilNavigation.Load });
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing && this.browser != null)
                {
                    this.browser.Dispose();
                }

                this.disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion
    }
}
