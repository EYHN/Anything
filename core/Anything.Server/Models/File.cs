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
        protected File(Application application, Url url, FileStats stats)
        {
            Application = application;
            Url = url;
            Stats = stats;
        }

        protected Application Application { get; }

        public Url Url { get; }

        public string Name => Url.Basename();

        public ValueTask<MimeType?> MimeType => Application.PreviewService.GetMimeType(Url, new MimeTypeOption());

        public ValueTask<string> IconId => Application.PreviewService.GetIconId(Url);

        public FileStats Stats { get; }

        public ValueTask<bool> IsSupportThumbnails => Application.PreviewService.IsSupportThumbnail(Url);

        public ValueTask<Metadata> Metadata => Application.PreviewService.GetMetadata(Url);
    }
}
