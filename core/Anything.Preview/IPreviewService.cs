using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Preview.MimeType;
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

        public ValueTask<MimeType.Schema.MimeType?> GetMimeType(Url url, MimeTypeOption option);

        public ValueTask<Metadata.Schema.Metadata> GetMetadata(Url url);
    }
}
