using System.IO;
using System.Threading.Tasks;
using OwnHub.File.Base;

namespace OwnHub.File.Local
{
    public class RegularFile : BaseRegularFile
    {
        private readonly FileInfo fileInfo;

        private readonly string pathName;

        public RegularFile(string pathName, FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            this.pathName = pathName;
        }

        public override string Path => pathName;

        public override string Name => PathUtils.Basename(pathName);

        public override Task<IFileStats?> Stats => Task.FromResult((IFileStats?) new FileStats(fileInfo));

        public string GetRealPath()
        {
            return fileInfo.FullName;
        }

        public override Stream Open()
        {
            return fileInfo.Open(FileMode.Open, FileAccess.Read);
        }
    }
}