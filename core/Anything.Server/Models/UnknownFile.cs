using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Server.Models
{
    public class UnknownFile : File
    {
        public UnknownFile(Application application, FileHandle fileHandle, FileStats stats)
            : base(application, fileHandle, stats)
        {
        }
    }
}
