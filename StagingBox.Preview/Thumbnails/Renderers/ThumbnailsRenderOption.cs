using StagingBox.FileSystem;
using StagingBox.Utils;

namespace StagingBox.Preview.Thumbnails.Renderers
{
    public record ThumbnailsRenderOption(Url Url)
    {
        public FileType FileType { get; init; }

        public string? MimeType { get; init; }

        public int Size { get; init; } = ThumbnailsConstants.DefaultSize;
    }
}
