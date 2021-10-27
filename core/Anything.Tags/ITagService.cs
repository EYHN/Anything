using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Tags
{
    public interface ITagService
    {
        public ValueTask<Tag[]> GetTags(FileHandle fileHandle);

        public ValueTask AddTags(FileHandle fileHandle, Tag[] tags);

        public ValueTask RemoveTags(FileHandle fileHandle, Tag[] tags);
    }
}
