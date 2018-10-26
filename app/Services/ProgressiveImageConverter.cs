using ImageMagick;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public interface IProgressiveImageConverter
    {
        void ConvertPngToProgressiveJpeg(string pngFilePath, string jpegFilePath, ProgressiveImageConverterOptions options);
    }

    public class ProgressiveImageConverterOptions
    {
        public bool Resize { get; set; } = false;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ProgressiveImageConverter : IProgressiveImageConverter
    {
        public void ConvertPngToProgressiveJpeg(string pngFilePath, string jpegFilePath,
            ProgressiveImageConverterOptions options)
        {
            using (var image = new MagickImage(pngFilePath))
            {
                image.Format = MagickFormat.Pjpeg;
                if (options.Resize)
                {
                    image.Resize(options.Width, options.Height);
                }
                image.Write(jpegFilePath);
            }
        }
    }
}
