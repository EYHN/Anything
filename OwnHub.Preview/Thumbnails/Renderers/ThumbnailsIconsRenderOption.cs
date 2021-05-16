using OwnHub.FileSystem;
using OwnHub.Utils;

namespace OwnHub.Preview.Thumbnails.Renderers
{
    public record ThumbnailsIconsRenderOption(Url Url, FileType FileType, string? MimeType);
}
