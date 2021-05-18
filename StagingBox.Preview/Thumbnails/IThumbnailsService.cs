using System.Threading.Tasks;
using StagingBox.Utils;

namespace StagingBox.Preview.Thumbnails
{
    public interface IThumbnailsService
    {
        public ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option);
    }
}
