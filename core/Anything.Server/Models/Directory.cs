using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Server.Models
{
    public class Directory : File
    {
        public Directory(Application application, FileHandle fileHandle, FileStats stats)
            : base(application, fileHandle, stats)
        {
        }

        public async ValueTask<IEnumerable<Dirent>> ReadEntries()
        {
            var entries = await Application.FileService.ReadDirectory(FileHandle.Value);
            return entries.Select(entry => new Dirent(entry.Name, Application.CreateFile(entry.FileHandle, entry.Stats)));
        }
    }
}
