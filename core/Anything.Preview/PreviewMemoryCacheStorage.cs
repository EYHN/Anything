using Anything.Database;
using Anything.Preview.Thumbnails.Cache;
using Anything.Utils;

namespace Anything.Preview
{
    public class PreviewMemoryCacheStorage : Disposable, IPreviewCacheStorage
    {
        private readonly SqliteContext _thumbnailsCacheSqliteContext;

        public PreviewMemoryCacheStorage()
        {
            _thumbnailsCacheSqliteContext = new SqliteContext();
            ThumbnailsCacheStorage = new ThumbnailsCacheDatabaseStorage(_thumbnailsCacheSqliteContext);
        }

        public IThumbnailsCacheStorage ThumbnailsCacheStorage { get; }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();
            _thumbnailsCacheSqliteContext.Dispose();
        }
    }
}
