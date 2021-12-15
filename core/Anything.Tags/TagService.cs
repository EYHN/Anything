using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Property;

namespace Anything.Tags;

public class TagService : ITagService
{
    private readonly IFileService _fileService;

    public TagService(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async ValueTask<Tag[]> GetTags(FileHandle fileHandle)
    {
        var tags = await _fileService.GetObjectProperty<string[]>(fileHandle, "tags");
        return tags == null ? Array.Empty<Tag>() : tags.Select(t => new Tag(t)).ToArray();
    }

    public async ValueTask SetTags(FileHandle fileHandle, IEnumerable<Tag> tags)
    {
        await _fileService.AddOrUpdateObjectProperty(fileHandle, "tags", tags.Select(t => t.Name).ToArray());
    }
}
