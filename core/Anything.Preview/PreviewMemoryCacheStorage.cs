using Anything.FileSystem;
using Anything.Preview.Thumbnails.Cache;
using Anything.Utils;
using Anything.Utils.Logging;

namespace Anything.Preview
{
    public class PreviewMemoryCacheStorage : Disposable, IPreviewCacheStorage
    {
        private readonly ThumbnailsCacheDatabaseStorage _thumbnailsCacheDatabaseStorage;

        public PreviewMemoryCacheStorage(IFileService fileService, ILogger logger)
        {
            _thumbnailsCacheDatabaseStorage = new ThumbnailsCacheDatabaseStorage(fileService, logger);
        }

        public IThumbnailsCacheStorage ThumbnailsCacheStorage => _thumbnailsCacheDatabaseStorage;

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _thumbnailsCacheDatabaseStorage.Dispose();
        }
    }
}
