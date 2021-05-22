using System.IO;

namespace Anything.Preview.Icons
{
    public class MemoryIcon : IIcon
    {
        public string ImageFormat { get; }

        public int Size { get; }

        private byte[] _data;

        public MemoryIcon(byte[] data, string imageFormat, int size)
        {
            _data = data;
            ImageFormat = imageFormat;
            Size = size;
        }

        public Stream GetStream()
        {
            return new MemoryStream(_data);
        }
    }
}
