using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails;
using Anything.Utils;

namespace Anything.Preview
{
    public interface IPreviewService
    {
        public ValueTask<IThumbnail?> GetThumbnails(Url url, ThumbnailOption option);

        public ValueTask<IIcon> GetIcons(Url url, IconsOption option);

        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option);
    }
}
