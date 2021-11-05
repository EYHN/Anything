using System;
using System.IO;
using Microsoft.Toolkit.HighPerformance;

namespace Anything.Preview.Icons
{
    public class MemoryIconImage : IIconImage
    {
        private readonly ReadOnlyMemory<byte> _data;

        public MemoryIconImage(ReadOnlyMemory<byte> data, string format, int size)
        {
            _data = data;
            Format = format;
            Size = size;
        }

        public string Format { get; }

        public int Size { get; }

        public Stream GetStream()
        {
            return _data.AsStream();
        }
    }
}
