using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails;
using Anything.Utils;

namespace Anything.Preview
{
    public interface IPreviewService
    {
        public ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option);

        public ValueTask<string> GetIconId(Url url);

        public ValueTask<IIconImage> GetIconImage(string iconId, IconImageOption option);

        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option);
    }
}
