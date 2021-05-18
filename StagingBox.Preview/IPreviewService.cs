using System.Threading.Tasks;
using StagingBox.Preview.Icons;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails;
using StagingBox.Utils;

namespace StagingBox.Preview
{
    public interface IPreviewService
    {
        public ValueTask<IThumbnail> GetThumbnails(Url url, ThumbnailOption option);

        public ValueTask<IIcon> GetIcons(Url url, IconsOption option);

        public ValueTask<string> GetMimeType(Url url, MimeTypeOption option);
    }
}
