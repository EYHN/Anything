using Anything.FileSystem;
using Anything.Preview.Mime.Schema;
using Anything.Utils;

namespace Anything.Preview.Meta.Readers
{
    public record MetadataReaderFileInfo(Url Url, FileStats Stats, MimeType? MimeType)
    {
        public FileType Type => Stats.Type;

        public long Size => Stats.Size;
    }
}
