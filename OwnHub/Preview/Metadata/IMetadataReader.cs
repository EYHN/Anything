using System.Threading.Tasks;
using OwnHub.File;

namespace OwnHub.Preview.Metadata
{
    public interface IMetadataReader
    {
        public string Name { get; }

        public bool IsSupported(IFile file);

        public Task<MetadataEntry> ReadMetadata(IFile file, MetadataEntry metadata);
    }
}