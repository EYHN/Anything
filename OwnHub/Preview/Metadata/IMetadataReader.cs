using System.Threading.Tasks;
using OwnHub.File;
using OwnHub.Test.Preview.Metadata;

namespace OwnHub.Preview.Metadata
{
    public interface IMetadataReader
    {
        public bool IsSupported(IFile file);

        public Task<MetadataEntry> ReadMetadata(IFile file, MetadataEntry metadata);
    }
}