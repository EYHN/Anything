using StagingBox.FileSystem;
using StagingBox.Preview.MimeType;
using StagingBox.Utils;

namespace StagingBox.Server.Models
{
    public class RegularFile : File
    {
        public RegularFile(Application application, Url url, FileStats stats)
            : base(application, url, stats)
        {
        }
    }
}
