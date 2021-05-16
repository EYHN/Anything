using System.IO;

namespace OwnHub.Preview.Icons
{
    public interface IIcons
    {
        public Stream ImageBuffer { get; }

        public string ImageType { get; }

        public int ImageHeight { get; }

        public int ImageWidth { get; }
    }
}
