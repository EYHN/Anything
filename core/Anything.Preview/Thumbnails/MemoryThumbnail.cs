using System.IO;
using SkiaSharp;

namespace Anything.Preview.Thumbnails
{
    public class MemoryThumbnail : IThumbnail
    {
        private readonly byte[] _data;

        public MemoryThumbnail(byte[] data, string imageType, int size)
        {
            Size = size;
            ImageFormat = imageType;
            _data = data;
        }

        public string ImageFormat { get; }

        public int Size { get; }

        public Stream GetStream()
        {
            return new MemoryStream(_data);
        }
    }
}
