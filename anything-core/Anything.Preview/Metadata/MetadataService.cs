using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Metadata.Readers;
using Anything.Preview.MimeType;
using Anything.Utils;

namespace Anything.Preview.Metadata
{
    public class MetadataService : IMetadataService
    {
        private readonly IFileSystemService _fileSystem;

        private readonly IMimeTypeService _mimeType;

        private readonly List<IMetadataReader> _readers = new();

        public MetadataService(IFileSystemService fileSystem, IMimeTypeService mimeType)
        {
            _fileSystem = fileSystem;
            _mimeType = mimeType;
        }

        public void RegisterRenderer(IMetadataReader renderer)
        {
            _readers.Add(renderer);
        }

        public async ValueTask<Schema.Metadata> ReadMetadata(Url url)
        {
            var stats = await _fileSystem.Stat(url);

            var mimeType = await _mimeType.GetMimeType(url, new MimeTypeOption());
            var fileInfo = new MetadataReaderFileInfo(url, stats, mimeType);

            var matchedReaders = _readers.Where(reader => reader.IsSupported(fileInfo));

            var readerOption = new MetadataReaderOption();
            var metadata = new Schema.Metadata();
            foreach (var reader in matchedReaders)
            {
                await reader.ReadMetadata(metadata, fileInfo, readerOption);
            }

            return metadata;
        }
    }
}
