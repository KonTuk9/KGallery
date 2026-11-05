using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KGallery.Images
{
    public static class Imaging
    {
        public static async Task<byte[]> CreateThumbnailPixelsAsync(string imagePath, int pixelWidth = 256)
        {
            return await Task.Run(() => CreateThumbnailPixels(imagePath, pixelWidth));
        }

        public static BitmapSource LoadImage(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            if (!File.Exists(path))
            {
                return null;
            }
            BitmapImage bitmap = new BitmapImage
            {
                CacheOption = BitmapCacheOption.OnLoad,
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            bitmap.Freeze();

            return bitmap;
        }

        public static byte[] CreateThumbnailPixels(string imagePath, int pixelWidth = 256)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imagePath);
            BitmapImage thumbnail = new BitmapImage();

            thumbnail.BeginInit();
            thumbnail.CacheOption = BitmapCacheOption.OnLoad;
            thumbnail.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            thumbnail.UriSource = new Uri(imagePath);
            thumbnail.DecodePixelWidth = pixelWidth;
            thumbnail.EndInit();
            thumbnail.Freeze();


            BitmapEncoder encoder = RecognizeFormat(imagePath);
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));

            using MemoryStream stream = new MemoryStream();
            encoder.Save(stream);

            return stream.ToArray();
        }

        public static BitmapSource GetImageFromBytes(byte[] pixels)
        {
            using MemoryStream stream = new MemoryStream(pixels);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }

        private static BitmapEncoder RecognizeFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".png" => new PngBitmapEncoder(),
                ".jpeg" => new JpegBitmapEncoder(),
                ".jpg" => new JpegBitmapEncoder(),
                ".bmp" => new BmpBitmapEncoder(),
                ".gif" => new GifBitmapEncoder(),
                ".tiff" => new TiffBitmapEncoder(),
                ".wmp" => new WmpBitmapEncoder(),
                _ => throw new InvalidOperationException()
            };
        }

        public static void SaveImageFromClipboard(string destinationPath)
        {
            if (!Clipboard.ContainsImage())
            {
                throw new InvalidOperationException();
            }

            BitmapSource source = Clipboard.GetImage();
            int pw = source.PixelWidth;
            int ph = source.PixelHeight;
            int stride = pw * 4;
            byte[] pixels = new byte[ph * stride];
            source.CopyPixels(pixels, stride, 0);

            BitmapSource image = BitmapSource.Create(pw, ph, source.DpiX, source.DpiY, PixelFormats.Bgra32, null, pixels, stride);
            image.Freeze();

            using FileStream fs = File.Create(destinationPath);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(fs);
        }
    }
}
