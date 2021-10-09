using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime;
using Anything.Preview.Mime.Schema;
using Anything.Utils;

namespace Anything.Server.Models
{
    public abstract class File
    {
        private readonly FileHandle _rawFileHandle;

        protected File(Application application, FileHandle fileHandle, FileStats stats)
        {
            Application = application;
            _rawFileHandle = fileHandle;
            Stats = stats;
        }

        protected Application Application { get; }

        public ValueTask<Url> GetUrl() => Application.FileService.GetUrl(_rawFileHandle);

        public ValueTask<string> GetFileName() => Application.FileService.GetFileName(_rawFileHandle);

        public FileHandleRef FileHandle => new(Application, _rawFileHandle);

        public ValueTask<MimeType?> GetMimeType() => Application.PreviewService.GetMimeType(_rawFileHandle, new MimeTypeOption());

        public ValueTask<string> GetIconId() => Application.PreviewService.GetIconId(_rawFileHandle);

        public FileStats Stats { get; }

        public ValueTask<bool> IsSupportThumbnails() => Application.PreviewService.IsSupportThumbnail(_rawFileHandle);

        public ValueTask<Metadata> GetMetadata() => Application.PreviewService.GetMetadata(_rawFileHandle);
    }
}
