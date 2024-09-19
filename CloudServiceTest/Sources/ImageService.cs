using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace CloudServiceTest
{
    public class ImageService
    {

        private readonly List<string> AllowedImageMimeTypes = new List<string>
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/bmp"
        };

        public bool IsImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            // Check MIME type
            if (!AllowedImageMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return false;
            }

            // Optionally: check file header (magic numbers) for further security
            using (var stream = file.OpenReadStream())
            {
                try
                {
                    byte[] header = new byte[4];
                    stream.Read(header, 0, 4);
                    return IsImageHeader(header);
                }
                catch
                {
                    return false;
                }
            }
        }

        private static bool IsImageHeader(byte[] header)
        {
            // JPEG
            if (header[0] == 0xFF && header[1] == 0xD8)
                return true;

            // PNG
            if (header[0] == 0x89 && header[1] == 0x50)
                return true;

            //// GIF
            //if (header[0] == 0x47 && header[1] == 0x49)
            //    return true;

            // BMP
            if (header[0] == 0x42 && header[1] == 0x4D)
                return true;

            return false;
        }


        public byte[] GenerateThumbnail(IFormFile originalImage, int sideLength = 128)
        {

            using (var originalImageStream = originalImage.OpenReadStream())
            {
                using (var image = Image.Load<Rgba32>(originalImageStream))
                {
                    int width = image.Width;
                    int height = image.Height;

                    bool isWide = width > height;
                    float ratio = (float)width / (float)height;

                    if(isWide)
                    {
                        width = sideLength;
                        height = (int)(sideLength / ratio);
                    }
                    else
                    {
                        height = sideLength;
                        width = (int)(sideLength * ratio);
                    }

                    image.Mutate(x => x.Resize(width, height));

                    using (var thumbnailStream = new MemoryStream())
                    {
                        image.Save(thumbnailStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                        return thumbnailStream.ToArray();
                    }
                }
            }
        }
    }
}
