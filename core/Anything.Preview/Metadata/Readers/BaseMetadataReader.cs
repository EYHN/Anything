using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Preview.Metadata.Readers
{
    public abstract class BaseMetadataReader : IMetadataReader
    {
        /// <summary>
        ///     Gets the mimetype supported by the reader.
        /// </summary>
        protected abstract string[] SupportMimeTypes { get; }

        /// <summary>
        ///     Gets the maximum file size supported by the reader.
        /// </summary>
        protected virtual long MaxFileSize => long.MaxValue;

        public bool IsSupported(MetadataReaderFileInfo fileInfo)
        {
            if (fileInfo.Type.HasFlag(FileType.File) && SupportMimeTypes.Contains(fileInfo.MimeType) && fileInfo.Size <= MaxFileSize)
            {
                return true;
            }

            return false;
        }

        protected abstract Task<Schema.Metadata> ReadMetadata(
            Schema.Metadata metadata,
            MetadataReaderFileInfo fileInfo,
            MetadataReaderOption option);

        async Task<Schema.Metadata> IMetadataReader.ReadMetadata(
            Schema.Metadata metadata,
            MetadataReaderFileInfo fileInfo,
            MetadataReaderOption option)
        {
            if (!IsSupported(fileInfo))
            {
                return metadata;
            }

            return await ReadMetadata(metadata, fileInfo, option);
        }
    }
}
