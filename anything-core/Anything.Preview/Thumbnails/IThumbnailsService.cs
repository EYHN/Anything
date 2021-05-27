using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.Preview.Thumbnails
{
    public interface IThumbnailsService
    {
        public ValueTask<bool> IsSupportThumbnail(Url url);

        public ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option);
    }
}
