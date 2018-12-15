using Microsoft.Extensions.Logging;
using MidnightLizard.Schemes.Screenshots.Models;
using MidnightLizard.Schemes.Screenshots.Services;
using Newtonsoft.Json;

namespace MidnightLizard.Schemes.Screenshots.EventHandlers
{
    public interface ISchemeUnpublishedEventHandler
    {
        void HandleEvent(string schemeUnpublishedEventJsonString);
    }

    public class SchemeUnpublishedEventHandler : ISchemeUnpublishedEventHandler
    {
        private readonly ILogger<SchemeUnpublishedEventHandler> logger;
        private readonly IScreenshotUploader screenshotUploader;

        public SchemeUnpublishedEventHandler(
            ILogger<SchemeUnpublishedEventHandler> logger,
            IScreenshotUploader screenshotUploader)
        {
            this.logger = logger;
            this.screenshotUploader = screenshotUploader;
        }

        public void HandleEvent(string schemeUnpublishedEventJsonString)
        {
            var unpublishEvent = JsonConvert.DeserializeObject<SchemeUnpublishedEvent>(schemeUnpublishedEventJsonString);
            try
            {
                screenshotUploader.DeleteScrenshots(unpublishEvent.Id);
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(ex, $"Failed to delete screenshots from CDN for PublicScheme [{unpublishEvent.Id}]");
            }
        }
    }
}
