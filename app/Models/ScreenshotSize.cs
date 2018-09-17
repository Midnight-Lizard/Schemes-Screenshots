using System.Linq;

namespace MidnightLizard.Schemes.Screenshots.Models
{
    public class ScreenshotSize
    {
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Scale { get; set; }

        public ScreenshotSize()
        {
        }

        public ScreenshotSize(string sizeStr)
        {
            var parts = sizeStr.Split(':');
            this.Title = parts[0];

            var size = parts[1].Split('x').Select(x => int.Parse(x)).ToArray();
            this.Width = size[0];
            this.Height = size[1];
            this.Scale = size[2];
        }

        public override string ToString()
        {
            return $"{this.Title}:{this.Width}x{this.Height}x{this.Scale}";
        }
    }
}
