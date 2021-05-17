using System.Threading.Tasks;
using OwnHub.Utils;

namespace OwnHub.Preview.Thumbnails
{
    public interface IThumbnailsService
    {
        public ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option);
    }
}
