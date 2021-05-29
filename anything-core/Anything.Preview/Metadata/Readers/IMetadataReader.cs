using System.Threading.Tasks;

namespace Anything.Preview.Metadata.Readers
{
    public interface IMetadataReader
    {
        public bool IsSupported(MetadataReaderFileInfo fileInfo);

        public Task<Schema.Metadata> ReadMetadata(Schema.Metadata metadata, MetadataReaderFileInfo fileInfo, MetadataReaderOption option);
    }
}
