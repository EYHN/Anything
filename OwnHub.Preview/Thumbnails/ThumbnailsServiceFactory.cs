using System.IO;
using System.Threading.Tasks;
using OwnHub.Database;
using OwnHub.FileSystem;
using OwnHub.Preview.MimeType;
using OwnHub.Preview.Thumbnails.Cache;
using OwnHub.Preview.Thumbnails.Renderers;

namespace OwnHub.Preview.Thumbnails
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
