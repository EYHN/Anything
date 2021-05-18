using System.Threading.Tasks;
using StagingBox.FileSystem;
using StagingBox.FileSystem.Exception;
using StagingBox.Preview;
using StagingBox.Preview.MimeType;
using StagingBox.Utils;

namespace StagingBox.Server.Models
{
    public class Application
    {
        public IFileSystemService FileSystemService { get; }

        public IPreviewService PreviewService { get; }

        public Application(IFileSystemService fileSystemService, IPreviewService previewService)
        {
            FileSystemService = fileSystemService;
            PreviewService = previewService;
        }

        public async ValueTask<Directory> OpenDirectory(Url url)
        {
            var stats = await FileSystemService.Stat(url);

            if (!stats.Type.HasFlag(FileType.Directory))
            {
                throw new FileNotADirectoryException(url);
            }

            return (CreateFile(url, stats) as Directory)!;
        }

        public async ValueTask<File> Open(Url url)
        {
            var stats = await FileSystemService.Stat(url);

            return CreateFile(url, stats);
        }

        public File CreateFile(Url url, FileStats stats)
        {
            if (stats.Type.HasFlag(FileType.File))
            {
                return new RegularFile(this, url, stats);
            }
            else if (stats.Type.HasFlag(FileType.Directory))
            {
                return new Directory(this, url, stats);
            }
            else
            {
                return new UnknownFile(this, url, stats);
            }
        }
    }
}
