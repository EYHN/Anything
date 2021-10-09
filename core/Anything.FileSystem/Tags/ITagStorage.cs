using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.FileSystem.Tags
{
    public interface ITagStorage
    {
        public ValueTask InsertTags(FileHandle fileHandle, Tag[] tags);

        public ValueTask<Tag[]> GetTag(FileHandle fileHandle);

        public ValueTask DeleteTag(FileHandle fileHandle, Tag[] tags);

        public ValueTask DeleteAllTagsBatch(FileHandle[] fileHandles);
    }
}
