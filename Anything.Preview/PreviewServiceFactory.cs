using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Icons;
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
