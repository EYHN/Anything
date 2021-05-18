using System.Threading.Tasks;
using StagingBox.Preview.Icons;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails;
using StagingBox.Utils;

namespace StagingBox.Preview
{
    public class PreviewService : IPreviewService
    {
        private readonly IThumbnailsService _thumbnailsService;

        private readonly IMimeTypeService _mimeTypeService;

        private readonly IIconsService _iconsService;

        public PreviewService(IThumbnailsService thumbnailsService, IMimeTypeService mimeTypeService, IIconsService iconsService)
        {
            _thumbnailsService = thumbnailsService;
            _mimeTypeService = mimeTypeService;
            _iconsService = iconsService;
        }

        public ValueTask<IThumbnail?> GetThumbnails(Url url, ThumbnailOption option)
        {
            return _thumbnailsService.GetThumbnail(url, option);
        }

        public ValueTask<IIcon> GetIcons(Url url, IconsOption option)
        {
            return _iconsService.GetIcon(url, option);
        }

        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option)
        {
            return _mimeTypeService.GetMimeType(url, option);
        }
    }
}
