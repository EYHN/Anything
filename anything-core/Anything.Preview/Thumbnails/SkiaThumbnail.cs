using System.IO;
using SkiaSharp;

namespace Anything.Preview.Thumbnails
{
    public class SkiaThumbnail : IThumbnail
    {
        public string ImageFormat { get; }

        public int Size { get; }

        private SKData _skData;

        public SkiaThumbnail(SKData data, string imageType, int size)
        {
            Size = size;
            ImageFormat = imageType;
            _skData = data;
        }

        public Stream GetStream()
        {
            return _skData.AsStream();
        }
    }
}
