using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Icons;
using Anything.Preview.Meta;
using Anything.Preview.Mime;
using Anything.Preview.Thumbnails;

namespace Anything.Preview
{
    public static class PreviewServiceFactory
    {
        public static async ValueTask<IPreviewService> BuildPreviewService(
            IFileService fileService,
            MimeTypeRules mimeTypeRules,
            IPreviewCacheStorage cacheStorage)
        {
            var mimeTypeService = new MimeTypeService(mimeTypeRules);
            var thumbnailsService = ThumbnailsServiceFactory.BuildThumbnailsService(
                fileService,
                mimeTypeService,
                cacheStorage.ThumbnailsCacheStorage);
            return new PreviewService(
                await IconsServiceFactory.BuildIconsService(fileService),
                mimeTypeService,
                thumbnailsService,
                MetadataServiceFactory.BuildMetadataService(fileService, mimeTypeService));
        }
    }
}
