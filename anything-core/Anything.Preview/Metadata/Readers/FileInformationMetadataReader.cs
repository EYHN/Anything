using System.Threading.Tasks;

namespace Anything.Preview.Metadata.Readers
{
    public class FileInformationMetadataReader : IMetadataReader
    {
        public bool IsSupported(MetadataReaderFileInfo fileInfo)
        {
            return true;
        }

        public Task<Schema.Metadata> ReadMetadata(Schema.Metadata metadata, MetadataReaderFileInfo fileInfo, MetadataReaderOption option)
        {
            var stats = fileInfo.Stats;

            if (stats != null)
            {
                metadata.Information.CreationTime = stats.CreationTime;
                metadata.Information.LastWriteTime = stats.LastWriteTime;
            }

            return Task.FromResult(metadata);
        }
    }
}
