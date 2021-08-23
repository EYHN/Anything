using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime;
using Anything.Preview.Mime.Schema;
using Anything.Preview.Thumbnails;
using Anything.Utils;

namespace Anything.Preview
{
    public interface IPreviewService
    {
        public ValueTask<bool> IsSupportThumbnail(Url url);

        public ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option);

        public ValueTask<string> GetIconId(Url url);

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option);

        public ValueTask<MimeType?> GetMimeType(Url url, MimeTypeOption option);

        public ValueTask<Metadata> GetMetadata(Url url);
    }
}
