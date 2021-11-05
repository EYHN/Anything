using System;
using System.IO;
using Microsoft.Toolkit.HighPerformance;

namespace Anything.Preview.Thumbnails
{
    public class MemoryThumbnail : IThumbnail
    {
        private readonly ReadOnlyMemory<byte> _data;

        public MemoryThumbnail(ReadOnlyMemory<byte> data, string imageType, int size)
        {
            Size = size;
            ImageFormat = imageType;
            _data = data;
        }

        public string ImageFormat { get; }

        public int Size { get; }

        public Stream GetStream()
        {
            return _data.AsStream();
        }
    }
}
