using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Exception;
using Anything.Utils;

namespace Anything.Server.Models
{
    public record FileHandleRef(Application Application, FileHandle Value)
    {
        public async ValueTask<Directory> OpenDirectory()
        {
            var file = await OpenFile();

            if (!(file is Directory directory))
            {
                throw new FileNotADirectoryException(Value);
            }

            return directory;
        }

        public async ValueTask<File> OpenFile()
        {
            var stats = await Application.FileService.Stat(Value);

            return Application.CreateFile(Value, stats);
        }
    }
}
