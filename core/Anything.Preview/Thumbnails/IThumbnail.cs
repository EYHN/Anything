using System.IO;

namespace Anything.Preview.Thumbnails
{
    public interface IThumbnail
    {
        public string ImageFormat { get; }

        public int Size { get; }

        public Stream GetStream();
    }
}
