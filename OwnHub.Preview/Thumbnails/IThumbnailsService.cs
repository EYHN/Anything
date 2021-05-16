using OwnHub.Utils;

namespace OwnHub.Preview.Thumbnails
{
    public interface IThumbnailsService
    {
        public IThumbnails GetThumbnail(Url url, ThumbnailOption option);
    }
}
