using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Preview.Thumbnails.Renderers
{
    public record ThumbnailsRenderFileInfo(Url Url, FileStats Stats, string? MimeType)
    {
        public FileType Type => Stats.Type;

        public long Size => Stats.Size;
    }
}
