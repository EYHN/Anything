using System.Threading.Tasks;
using StagingBox.FileSystem;
using StagingBox.Preview.MimeType;
using StagingBox.Utils;

namespace StagingBox.Server.Models
{
    public abstract class File
    {
        protected Application Application { get; }

        public Url Url { get; }

        public string Name => Url.Basename();

        public ValueTask<string?> MimeType => Application.PreviewService.GetMimeType(Url, new MimeTypeOption());

        public FileStats Stats { get; }

        public File(Application application, Url url, FileStats stats)
        {
            Application = application;
            Url = url;
            Stats = stats;
        }
    }
}
