using System.Threading.Tasks;
using OwnHub.Utils;

namespace OwnHub.Preview.Thumbnails.Cache
{
    public interface IThumbnailsCacheStorage
    {
        public ValueTask Cache(Url url, string tag, string key, byte[] data);

        public ValueTask<byte[]?> GetCache(Url url, string tag, string key);

        public ValueTask Delete(Url url);
    }
}
