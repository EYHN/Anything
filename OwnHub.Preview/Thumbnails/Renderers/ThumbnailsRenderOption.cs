using OwnHub.FileSystem;
using OwnHub.Utils;

namespace OwnHub.Preview.Thumbnails.Renderers
{
    public record ThumbnailsRenderOption(Url Url)
    {
        public FileType FileType { get; init; }

        public string? MimeType { get; init; }

        public int Size { get; init; } = ThumbnailsConstants.DefaultSize;
    }
}
