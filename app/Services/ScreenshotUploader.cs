using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Models;
using System;
using System.Text.RegularExpressions;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public interface IScreenshotUploader
    {
        void UploadScreenshot(Screenshot screenshot);
        void DeleteScrenshots(string aggregateId);
    }

    public class ScreenshotUploader : IScreenshotUploader
    {
        private readonly Cloudinary cloudinary;
        private readonly IOptions<ScreenshotsConfig> screenshotConfig;

        public ScreenshotUploader(IOptions<ScreenshotsConfig> screenshotConfig)
        {
            this.cloudinary = new Cloudinary();
            this.screenshotConfig = screenshotConfig;
        }

        public void UploadScreenshot(Screenshot screenshot)
        {
            var title = Regex.Replace(screenshot.Title.ToLower(), "\\s", "-");
            var publicId = this.screenshotConfig.Value.SCREENSHOT_CDN_ID_TEMPLATE
                .Replace("{id}", screenshot.AggregateId)
                .Replace("{title}", title)
                .Replace("{size}", screenshot.Size.ToString());
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(screenshot.FilePath),
                PublicId = publicId,
            };
            var uploadResult = this.cloudinary.Upload(uploadParams);
            if (uploadResult.Error != null)
            {
                throw new ApplicationException($"Faild to upload screenshot for {screenshot.AggregateId}. Error: {uploadResult.Error.Message}");
            }
        }

        public void DeleteScrenshots(string aggregateId)
        {
            var publicIdPrefix = this.screenshotConfig.Value.SCREENSHOT_CDN_PREFIX_TEMPLATE
                .Replace("{id}", aggregateId);
            var deleteResult = this.cloudinary.DeleteResourcesByPrefix(publicIdPrefix);
            if (deleteResult.Error != null)
            {
                throw new ApplicationException($"Faild to delete screenshots for {aggregateId}. Error: {deleteResult.Error.Message}");
            }
        }
    }
}
