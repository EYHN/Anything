using StagingBox.FileSystem;
using StagingBox.Utils;

namespace StagingBox.Server.Models
{
    public class UnknownFile : File
    {
        public UnknownFile(Application application, Url url, FileStats stats)
            : base(application, url, stats)
        {
        }
    }
}
