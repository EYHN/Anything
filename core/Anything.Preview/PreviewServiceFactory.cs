using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Icons;
using Anything.Preview.Metadata;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails;

namespace Anything.Preview
{
    public static class PreviewServiceFactory
    {
        public static async ValueTask<IPreviewService> BuildPreviewService(
            IFileService fileService,
            MimeTypeRules mimeTypeRules,
            string cachePath)
        {
            var mimeTypeService = new MimeTypeService(mimeTypeRules);
            var thumbnailsService = ThumbnailsServiceFactory.BuildThumbnailsService(
                fileService,
                mimeTypeService,
                cachePath);
            return new PreviewService(
                await IconsServiceFactory.BuildIconsService(fileService),
                mimeTypeService,
                thumbnailsService,
                MetadataServiceFactory.BuildMetadataService(fileService, mimeTypeService));
        }
    }
}
