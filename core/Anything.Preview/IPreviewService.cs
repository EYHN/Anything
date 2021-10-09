using System.Threading.Tasks;
using Anything.FileSystem;
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
        public ValueTask<bool> IsSupportThumbnail(FileHandle fileHandle);

        public ValueTask<IThumbnail?> GetThumbnail(FileHandle fileHandle, ThumbnailOption option);

        public ValueTask<string> GetIconId(FileHandle fileHandle);

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option);

        public ValueTask<MimeType?> GetMimeType(FileHandle fileHandle, MimeTypeOption option);

        public ValueTask<Metadata> GetMetadata(FileHandle fileHandle);
    }
}
