using System.IO;
using System.Threading.Tasks;
using StagingBox.Database;
using StagingBox.FileSystem;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails.Cache;
using StagingBox.Preview.Thumbnails.Renderers;

namespace StagingBox.Preview.Thumbnails
{
    public static class ThumbnailsServiceFactory
    {
        public static async ValueTask<IThumbnailsService> BuildThumbnailsService(
            IFileSystemService fileSystem,
            IMimeTypeService mimeType,
            string cachePath)
        {
            Directory.CreateDirectory(Path.Join(cachePath, "thumbnails"));
            var cacheStorage = new ThumbnailsCacheDatabaseStorage(new SqliteContext(Path.Join(cachePath, "thumbnails", "cache.db")));
            await cacheStorage.Create();
            ThumbnailsCacheStorageAutoCleanUp.RegisterAutoCleanUp(cacheStorage, fileSystem);
            var service = new ThumbnailsService(fileSystem, mimeType, cacheStorage);
            service.RegisterRenderer(new ImageFileRenderer(fileSystem));
            service.RegisterRenderer(new TextFileRenderer(fileSystem));
            return service;
        }
    }
}
