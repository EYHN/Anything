using Anything.FileSystem;
using Anything.Preview.Mime;
using Anything.Preview.Thumbnails.Cache;
using Anything.Preview.Thumbnails.Renderers;

namespace Anything.Preview.Thumbnails
{
    public static class ThumbnailsServiceFactory
    {
        public static ThumbnailsService BuildThumbnailsService(
            IFileService fileService,
            IMimeTypeService mimeType,
            IThumbnailsCacheStorage cacheStorage)
        {
            var service = new ThumbnailsService(fileService, mimeType, cacheStorage);
            service.RegisterRenderer(new ImageFileRenderer(fileService));
            service.RegisterRenderer(new TextFileRenderer(fileService));
            return service;
        }
    }
}
