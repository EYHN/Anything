using System.Threading.Tasks;
using StagingBox.FileSystem;
using StagingBox.Preview.Icons;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails;

namespace StagingBox.Preview
{
    public static class PreviewServiceFactory
    {
        public static async ValueTask<IPreviewService> BuildPreviewService(
            IFileSystemService fileSystemService,
            MimeTypeRules mimeTypeRules,
            string cachePath)
        {
            var mimeTypeService = new MimeTypeService(mimeTypeRules);
            return new PreviewService(
                await ThumbnailsServiceFactory.BuildThumbnailsService(
                    fileSystemService,
                    mimeTypeService,
                    cachePath),
                mimeTypeService,
                await IconsServiceFactory.BuildIconsService(fileSystemService));
        }
    }
}
