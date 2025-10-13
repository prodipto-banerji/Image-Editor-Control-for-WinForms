using System;
using System.Drawing;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CraftSynth.ImageEditor
{
    internal static class ImageSharpHelper
    {
        public static Bitmap LoadBitmapFromFile(string path)
        {
            using var image = SixLabors.ImageSharp.Image.Load(path);
            return ToBitmap(image);
        }

        public static Bitmap LoadBitmapFromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var image = SixLabors.ImageSharp.Image.Load(ms);
            return ToBitmap(image);
        }

        public static Bitmap ResizeBitmap(Bitmap source, int width, int height)
        {
            // Convert source to ImageSharp, resize, convert back
            using var srcStream = new MemoryStream();
            source.Save(srcStream, System.Drawing.Imaging.ImageFormat.Png);
            srcStream.Position = 0;
            using var image = SixLabors.ImageSharp.Image.Load(srcStream);
            image.Mutate(ctx => ctx.Resize(width, height));
            return ToBitmap(image);
        }

        public static void SaveBitmapToFile(Bitmap bitmap, string filePath)
        {
            var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
            using var srcStream = new MemoryStream();
            bitmap.Save(srcStream, System.Drawing.Imaging.ImageFormat.Png);
            srcStream.Position = 0;
            using var image = SixLabors.ImageSharp.Image.Load(srcStream);
            IImageEncoder encoder = ext switch
            {
                ".jpg" => new JpegEncoder { Quality = 90 },
                ".jpeg" => new JpegEncoder { Quality = 90 },
                ".png" => new PngEncoder { ColorType = PngColorType.RgbWithAlpha },
                ".gif" => new GifEncoder(),
                ".bmp" => new BmpEncoder(),
                _ => new PngEncoder(),
            };
            using var outStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            image.Save(outStream, encoder);
        }

        private static Bitmap ToBitmap(SixLabors.ImageSharp.Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            ms.Position = 0;
            return new Bitmap(ms);
        }
    }
}
