using System.IO;

namespace Anything.Preview.Icons
{
    public interface IIconImage
    {
        public string Format { get; }

        public int Size { get; }

        public Stream GetStream();
    }
}
