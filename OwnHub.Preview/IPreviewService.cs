using System.Threading.Tasks;
using OwnHub.Preview.Icons;
using OwnHub.Preview.MimeType;
using OwnHub.Preview.Thumbnails;
using OwnHub.Utils;

namespace OwnHub.Preview
{
    public interface IPreviewService
    {
        public ValueTask<IThumbnail> GetThumbnails(Url url, ThumbnailOption option);

        public ValueTask<IIcons> GetIcons(Url url, IconsOption option);

        public ValueTask<string> GetMimeType(Url url, MimeTypeOption option);
    }
}
