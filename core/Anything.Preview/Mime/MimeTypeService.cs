using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Mime.Schema;

namespace Anything.Preview.Mime;

public class MimeTypeService : IMimeTypeService
{
    private readonly IFileService _fileService;
    private readonly MimeTypeRules _rules;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MimeTypeService" /> class.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    /// <param name="rules">Mime type rules.</param>
    public MimeTypeService(IFileService fileService, MimeTypeRules rules)
    {
        _rules = rules;
        _fileService = fileService;
    }

    /// <inheritdoc />
    public async ValueTask<MimeType?> GetMimeType(FileHandle fileHandle)
    {
        var fileName = await _fileService.GetFileName(fileHandle);
        if (fileName == null)
        {
            throw new NotSupportedException();
        }

        return _rules.Match(fileName);
    }
}
