using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Server.Models
{
    public class UnknownFile : File
    {
        public UnknownFile(Application application, Url url, FileStats stats)
            : base(application, url, stats)
        {
        }
    }
}
