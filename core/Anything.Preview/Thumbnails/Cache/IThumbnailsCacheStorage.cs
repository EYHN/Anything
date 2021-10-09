using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Preview.Thumbnails.Cache
{
    public interface IThumbnailsCacheStorage
    {
        public ValueTask Cache(FileHandle fileHandle, FileHash fileHash, IThumbnail thumbnail);

        public ValueTask<IThumbnail[]> GetCache(FileHandle fileHandle, FileHash fileHash);
    }
}
