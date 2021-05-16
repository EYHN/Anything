using System.IO;

namespace OwnHub.Preview.Thumbnails
{
    public interface IThumbnails
    {
        public Stream ImageBuffer { get; }

        public string ImageType { get; }

        public int ImageHeight { get; }

        public int ImageWidth { get; }
    }
}
