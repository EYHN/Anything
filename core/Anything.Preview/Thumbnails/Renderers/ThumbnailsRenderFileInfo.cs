using Anything.FileSystem;
using Anything.Preview.Mime.Schema;
using Anything.Utils;

namespace Anything.Preview.Thumbnails.Renderers
{
    public record ThumbnailsRenderFileInfo(FileHandle FileHandle, FileStats Stats, MimeType? MimeType)
    {
        public FileType Type => Stats.Type;

        public long Size => Stats.Size;
    }
}
