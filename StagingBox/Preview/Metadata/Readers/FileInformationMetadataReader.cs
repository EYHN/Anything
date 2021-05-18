using System.Threading.Tasks;
using StagingBox.File;

namespace StagingBox.Preview.Metadata.Readers
{
    public class FileInformationMetadataReader : IMetadataReader
    {
        public string Name { get; } = "FileInformationMetadataReader";
        public bool IsSupported(IFile file)
        {
            return file is IRegularFile || file is IDirectory;
        }

        public async Task<MetadataEntry> ReadMetadata(IFile file, MetadataEntry metadata)
        {
            if (!IsSupported(file)) return metadata;

            IFileStats? stats = await file.Stats;

            if (stats != null)
            {
                metadata.Information.CreationTime = stats.CreationTime;
                metadata.Information.ModifyTime = stats.ModifyTime;
            }

            return metadata;
        }
    }
}
