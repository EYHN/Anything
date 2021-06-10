using System.IO;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails.Cache;
using Anything.Preview.Thumbnails.Renderers;

namespace Anything.Preview.Thumbnails
{
    public static class ThumbnailsServiceFactory
    {
        public static async ValueTask<IThumbnailsService> BuildThumbnailsService(
            IFileService fileService,
            IMimeTypeService mimeType,
            string cachePath)
        {
            Directory.CreateDirectory(Path.Join(cachePath, "thumbnails"));
            var cacheStorage = new ThumbnailsCacheDatabaseStorage(new SqliteContext(Path.Join(cachePath, "thumbnails", "cache.db")));
            await cacheStorage.Create();
            ThumbnailsCacheStorageAutoCleanUp.RegisterAutoCleanUp(cacheStorage, fileService);
            var service = new ThumbnailsService(fileService, mimeType, cacheStorage);
            service.RegisterRenderer(new ImageFileRenderer(fileService));
            service.RegisterRenderer(new TextFileRenderer(fileService));
            return service;
        }
    }
}
