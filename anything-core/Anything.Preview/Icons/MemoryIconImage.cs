using System.IO;

namespace Anything.Preview.Icons
{
    public class MemoryIconImage : IIconImage
    {
        private readonly byte[] _data;

        public MemoryIconImage(byte[] data, string format, int size)
        {
            _data = data;
            Format = format;
            Size = size;
        }

        public string Format { get; }

        public int Size { get; }

        public Stream GetStream()
        {
            return new MemoryStream(_data);
        }
    }
}
