using Anything.Database;
using Anything.FileSystem;
using Anything.Preview.Thumbnails.Cache;
using Anything.Utils;

namespace Anything.Preview
{
    public class PreviewMemoryCacheStorage : Disposable, IPreviewCacheStorage
    {
        private readonly ThumbnailsCacheDatabaseStorage _thumbnailsCacheDatabaseStorage;

        public PreviewMemoryCacheStorage(IFileService fileService)
        {
            _thumbnailsCacheDatabaseStorage = new ThumbnailsCacheDatabaseStorage(fileService);
        }

        public IThumbnailsCacheStorage ThumbnailsCacheStorage => _thumbnailsCacheDatabaseStorage;

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _thumbnailsCacheDatabaseStorage.Dispose();
        }
    }
}
