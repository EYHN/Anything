using System.IO;

namespace StagingBox.Preview.Thumbnails
{
    public interface IThumbnail
    {
        public string ImageFormat { get; }

        public int Size { get; }

        public Stream GetStream();
    }
}
