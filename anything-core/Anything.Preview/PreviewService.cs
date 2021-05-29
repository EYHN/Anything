using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Preview.Metadata;
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

        private readonly IMetadataService _metadataService;

        public PreviewService(IIconsService iconsService, IMimeTypeService mimeTypeService, IThumbnailsService thumbnailsService, IMetadataService metadataService)
        {
            _iconsService = iconsService;
            _mimeTypeService = mimeTypeService;
            _thumbnailsService = thumbnailsService;
            _metadataService = metadataService;
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

        public ValueTask<Metadata.Schema.Metadata> GetMetadata(Url url)
        {
            return _metadataService.ReadMetadata(url);
        }
    }
}
