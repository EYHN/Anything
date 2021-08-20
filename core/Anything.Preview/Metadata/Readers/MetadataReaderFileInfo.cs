using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Preview.Metadata.Readers
{
    public record MetadataReaderFileInfo(Url Url, FileStats Stats, MimeType.Schema.MimeType? MimeType)
    {
        public FileType Type => Stats.Type;

        public long Size => Stats.Size;
    }
}
