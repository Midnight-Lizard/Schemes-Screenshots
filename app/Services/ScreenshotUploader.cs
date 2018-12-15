﻿using CloudinaryDotNet;
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
        void DeleteScrenshots(string publicSchemeId);
    }

    public class ScreenshotUploader : IScreenshotUploader
    {
        private readonly Cloudinary cloudinary;
        private readonly ScreenshotsConfig screenshotConfig;

        public ScreenshotUploader(IOptions<ScreenshotsConfig> screenshotConfig)
        {
            this.cloudinary = new Cloudinary();
            this.screenshotConfig = screenshotConfig.Value;
        }

        public void UploadScreenshot(Screenshot screenshot)
        {
            var title = Regex.Replace(screenshot.Title.ToLower(), "\\s", "-");
            var publicId = this.screenshotConfig.SCREENSHOT_CDN_ID_TEMPLATE
                .Replace("{id}", screenshot.PublicSchemeId)
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
                throw new ApplicationException($"Faild to upload screenshot for {screenshot.PublicSchemeId}. Error: {uploadResult.Error.Message}");
            }
        }

        public void DeleteScrenshots(string publicSchemeId)
        {
            var publicIdPrefix = this.screenshotConfig.SCREENSHOT_CDN_PREFIX_TEMPLATE
                .Replace("{id}", publicSchemeId);
            var deleteResult = this.cloudinary.DeleteResourcesByPrefix(publicIdPrefix);
            if (deleteResult.Error != null)
            {
                throw new ApplicationException($"Faild to delete screenshots for {publicSchemeId}. Error: {deleteResult.Error.Message}");
            }
        }
    }
}
