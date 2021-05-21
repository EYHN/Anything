using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;
using Anything.Preview.MimeType;

namespace Anything.Server.Models
{
    public class Directory : File
    {
        public ValueTask<IEnumerable<File>> Entries => ReadEntries();

        public Directory(Application application, Url url, FileStats stats)
            : base(application, url, stats)
        {
        }

        private async ValueTask<IEnumerable<File>> ReadEntries()
        {
            var entries = await Application.FileSystemService.ReadDirectory(Url);
            return entries.Select((entry) => Application.CreateFile(Url.JoinPath(entry.Name), entry.Stats));
        }
    }
}
