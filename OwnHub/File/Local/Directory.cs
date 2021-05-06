using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OwnHub.File.Base;
using OwnHub.Utils;

namespace OwnHub.File.Local
{
    public class Directory : BaseDirectory
    {
        private readonly DirectoryInfo directoryInfo;
        private readonly string pathName;

        public Directory(string pathName, DirectoryInfo directoryInfo)
        {
            this.pathName = pathName;
            this.directoryInfo = directoryInfo;
        }

        public override Task<IEnumerable<IFile>> Entries => Task.FromResult(EnumerateEntries());

        public override string Path => pathName;

        public override string Name => PathLib.Basename(pathName);

        public override Task<IFileStats?> Stats => Task.FromResult((IFileStats?) new FileStats(directoryInfo));

        private IEnumerable<IFile> EnumerateEntries()
        {
            foreach (var dir in directoryInfo.EnumerateDirectories())
                yield return new Directory(PathLib.Join(pathName, dir.Name), dir);
            foreach (var file in directoryInfo.EnumerateFiles())
                yield return new RegularFile(PathLib.Join(pathName, file.Name), file);
        }
    }
}
