using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StagingBox.FileSystem;
using StagingBox.Preview.MimeType;
using StagingBox.Utils;

namespace StagingBox.Server.Models
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
