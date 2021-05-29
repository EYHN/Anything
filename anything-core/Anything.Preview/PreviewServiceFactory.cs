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
            IFileSystemService fileSystemService,
            MimeTypeRules mimeTypeRules,
            string cachePath)
        {
            var mimeTypeService = new MimeTypeService(mimeTypeRules);
            var thumbnailsService = await ThumbnailsServiceFactory.BuildThumbnailsService(
                fileSystemService,
                mimeTypeService,
                cachePath);
            return new PreviewService(
                await IconsServiceFactory.BuildIconsService(fileSystemService),
                mimeTypeService,
                thumbnailsService,
                MetadataServiceFactory.BuildMetadataService(fileSystemService, mimeTypeService));
        }
    }
}
