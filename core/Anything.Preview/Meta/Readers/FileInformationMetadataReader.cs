using System.Threading.Tasks;
using Anything.Preview.Meta.Schema;

namespace Anything.Preview.Meta.Readers
{
    public class FileInformationMetadataReader : IMetadataReader
    {
        public bool IsSupported(MetadataReaderFileInfo fileInfo)
        {
            return true;
        }

        public Task<Metadata> ReadMetadata(Metadata metadata, MetadataReaderFileInfo fileInfo, MetadataReaderOption option)
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
