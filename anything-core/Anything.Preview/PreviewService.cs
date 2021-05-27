using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails;
using Anything.Utils;

namespace Anything.Preview
{
    public class PreviewService : IPreviewService
    {
        private readonly IIconsService _iconsService;

        private readonly IMimeTypeService _mimeTypeService;

        private readonly IThumbnailsService _thumbnailsService;

        public PreviewService(IThumbnailsService thumbnailsService, IMimeTypeService mimeTypeService, IIconsService iconsService)
        {
            _thumbnailsService = thumbnailsService;
            _mimeTypeService = mimeTypeService;
            _iconsService = iconsService;
        }

        public ValueTask<bool> IsSupportThumbnail(Url url)
        {
            return _thumbnailsService.IsSupportThumbnail(url);
        }

        public ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option)
        {
            return _thumbnailsService.GetThumbnail(url, option);
        }

        public ValueTask<string> GetIconId(Url url)
        {
            return _iconsService.GetIconId(url);
        }

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option)
        {
            return _iconsService.GetIconImage(id, option);
        }

        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option)
        {
            return _mimeTypeService.GetMimeType(url, option);
        }
    }
}
