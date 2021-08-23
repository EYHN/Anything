using System.Threading.Tasks;
using Anything.Preview.Meta.Schema;

namespace Anything.Preview.Meta.Readers
{
    public interface IMetadataReader
    {
        public bool IsSupported(MetadataReaderFileInfo fileInfo);

        public Task<Metadata> ReadMetadata(Metadata metadata, MetadataReaderFileInfo fileInfo, MetadataReaderOption option);
    }
}
