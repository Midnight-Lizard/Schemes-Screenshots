namespace MidnightLizard.Schemes.Screenshots.Models
{
    public class Screenshot
    {
        public string Title { get; set; }
        public string PublicSchemeId { get; set; }
        public ScreenshotSize Size { get; set; }
        public string Url { get; set; }
        public string FilePath { get; set; }
    }
}
