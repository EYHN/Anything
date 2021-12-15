using System.Collections.Generic;
using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Tags;

public interface ITagService
{
    public ValueTask<Tag[]> GetTags(FileHandle fileHandle);

    public ValueTask SetTags(FileHandle fileHandle, IEnumerable<Tag> tags);
}
