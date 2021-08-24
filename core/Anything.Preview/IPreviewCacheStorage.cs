using Anything.Preview.Thumbnails.Cache;

namespace Anything.Preview
{
    public interface IPreviewCacheStorage
    {
        IThumbnailsCacheStorage ThumbnailsCacheStorage { get; }
    }
}
