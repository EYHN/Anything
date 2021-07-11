using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Preview.Thumbnails.Cache
{
    public interface IThumbnailsCacheStorage
    {
        public ValueTask<long> Cache(Url url, FileRecord fileRecord, IThumbnail thumbnail);

        public ValueTask<IThumbnail[]> GetCache(Url url, FileRecord fileRecord);

        public ValueTask Delete(long id);

        public ValueTask DeleteBatch(long[] ids);
    }
}
