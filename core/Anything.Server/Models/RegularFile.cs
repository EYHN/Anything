using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Server.Models
{
    public class RegularFile : File
    {
        public RegularFile(Application application, FileHandle fileHandle, FileStats stats)
            : base(application, fileHandle, stats)
        {
        }
    }
}
