using System.Collections.Immutable;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime.Schema;

namespace Anything.Preview.Meta.Readers;

public abstract class BaseMetadataReader : IMetadataReader
{
    /// <summary>
    ///     Gets the mimetype supported by the reader.
    /// </summary>
    protected abstract ImmutableArray<MimeType> SupportMimeTypes { get; }

    /// <summary>
    ///     Gets the maximum file size supported by the reader.
    /// </summary>
    protected virtual long MaxFileSize => long.MaxValue;

    public bool IsSupported(MetadataReaderFileInfo fileInfo)
    {
        if (fileInfo.Type.HasFlag(FileType.File) && fileInfo.MimeType != null && SupportMimeTypes.Contains(fileInfo.MimeType) &&
            fileInfo.Size <= MaxFileSize)
        {
            return true;
        }

        return false;
    }

    async Task<Metadata> IMetadataReader.ReadMetadata(
        Metadata metadata,
        MetadataReaderFileInfo fileInfo,
        MetadataReaderOption option)
    {
        if (!IsSupported(fileInfo))
        {
            return metadata;
        }

        return await ReadMetadata(metadata, fileInfo, option);
    }

    protected abstract Task<Metadata> ReadMetadata(
        Metadata metadata,
        MetadataReaderFileInfo fileInfo,
        MetadataReaderOption option);
}
