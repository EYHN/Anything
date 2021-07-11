using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Server.Models
{
    public class Directory : File
    {
        public Directory(Application application, Url url, FileStats stats)
            : base(application, url, stats)
        {
        }

        public ValueTask<IEnumerable<File>> Entries => ReadEntries();

        private async ValueTask<IEnumerable<File>> ReadEntries()
        {
            var entries = await Application.FileService.ReadDirectory(Url);
            return entries.Select(entry => Application.CreateFile(Url.JoinPath(entry.Name), entry.Stats));
        }
    }
}
