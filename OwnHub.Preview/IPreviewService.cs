using OwnHub.Preview.Icons;
using OwnHub.Preview.MimeType;
using OwnHub.Preview.Thumbnails;
using OwnHub.Utils;

namespace OwnHub.Preview
{
    public interface IPreviewService
    {
        public IThumbnails GetThumbnails(Url url, ThumbnailOption option);

        public IThumbnails GetIcons(Url url, IconsOption option);

        public IThumbnails GetMimeType(Url url, MimeTypeOption option);
    }
}
