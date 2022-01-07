using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Meta.Readers;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime;

namespace Anything.Preview.Meta;

public class MetadataService : IMetadataService
{
    private readonly IFileService _fileService;

    private readonly IMimeTypeService _mimeType;

    private readonly ImmutableArray<IMetadataReader> _readers;

    public MetadataService(IFileService fileService, IMimeTypeService mimeType, IEnumerable<IMetadataReader> readers)
    {
        _readers = readers.ToImmutableArray();
        _fileService = fileService;
        _mimeType = mimeType;
    }

    public async ValueTask<Metadata> ReadMetadata(FileHandle fileHandle)
    {
        var stats = await _fileService.Stat(fileHandle);

        var mimeType = await _mimeType.GetMimeType(fileHandle);
        var fileInfo = new MetadataReaderFileInfo(fileHandle, stats, mimeType);

        var matchedReaders = _readers.Where(reader => reader.IsSupported(fileInfo));

        var readerOption = new MetadataReaderOption();
        var metadata = new Metadata();
        foreach (var reader in matchedReaders)
        {
            try
            {
                await reader.ReadMetadata(metadata, fileInfo, readerOption);
            }
            catch (Exception)
            {
                Console.WriteLine("ReadMetadata Error: " + reader.GetType().Name);
            }
        }

        return metadata;
    }
}
