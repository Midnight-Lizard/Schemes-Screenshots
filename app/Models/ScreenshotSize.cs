using System.Linq;

namespace MidnightLizard.Schemes.Screenshots.Models
{
    public class ScreenshotSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Scale { get; set; }

        public ScreenshotSize()
        {
        }

        public ScreenshotSize(string sizeStr)
        {
            var size = sizeStr.Split('x').Select(x => int.Parse(x)).ToArray();
            this.Width = size[0];
            this.Height = size[1];
            this.Scale = size[2];
        }

        public override string ToString()
        {
            return $"{this.Width}x{this.Height}";
        }
    }
}
