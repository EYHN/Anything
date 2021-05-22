using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails;
using Anything.Utils;

namespace Anything.Preview
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
