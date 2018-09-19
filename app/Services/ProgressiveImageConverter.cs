using ImageMagick;
using System.IO;

namespace MidnightLizard.Schemes.Screenshots.Services
{
    public interface IProgressiveImageConverter
    {
        void ConvertPngToProgressiveJpeg(string pngFilePath, string jpegFilePath);
    }

    public class ProgressiveImageConverter : IProgressiveImageConverter
    {
        public void ConvertPngToProgressiveJpeg(string pngFilePath, string jpegFilePath)
        {
            using (MagickImage image = new MagickImage(pngFilePath))
            {
                image.Format = MagickFormat.Pjpeg;
                image.Write(jpegFilePath);
            }
        }
    }
}
