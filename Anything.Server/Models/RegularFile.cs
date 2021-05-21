using Anything.FileSystem;
using Anything.Utils;
using Anything.Preview.MimeType;

namespace Anything.Server.Models
{
    public class RegularFile : File
    {
        public RegularFile(Application application, Url url, FileStats stats)
            : base(application, url, stats)
        {
        }
    }
}
