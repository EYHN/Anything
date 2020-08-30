using OwnHub.File.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File.Local
{
    public class Directory : BaseDirectory
    {
        DirectoryInfo directoryInfo;
        string pathName;

        public override Task<IEnumerable<IFile>> Entries => Task.FromResult(EnumerateEntries());

        public override string Path => pathName;

        public override string Name => PathUtils.Basename(pathName);

        public override Task<IFileStats> Stats => Task.FromResult((IFileStats)new FileStats(this.directoryInfo));

        public Directory(string pathName, DirectoryInfo directoryInfo)
        {
            this.pathName = pathName;
            this.directoryInfo = directoryInfo;
        }

        IEnumerable<IFile> EnumerateEntries()
        {
            foreach (var dir in directoryInfo.EnumerateDirectories())
            {
                yield return new Directory(PathUtils.Join(pathName, dir.Name), dir);
            }
            foreach (var file in directoryInfo.EnumerateFiles())
            {
                yield return new RegularFile(PathUtils.Join(pathName, file.Name), file);
            }
        }
    }
}
