using System.Collections.Generic;
using System.Threading.Tasks;
using OwnHub.File.Base;

namespace OwnHub.File.Virtual
{
    public class Directory : BaseDirectory
    {
        public string PathName = null!;
        public IEnumerable<IFile> Child = null!;
        
        public Directory()
        {
        }

        public Directory(string pathName, IEnumerable<IFile> entries)
        {
            PathName = pathName;
            Child = entries;
        }

        public override Task<IEnumerable<IFile>> Entries => Task.FromResult(EnumerateEntries());

        public override string Path => PathName;

        public override string Name => PathUtils.Basename(PathName);

        public override Task<IFileStats?> Stats => Task.FromResult((IFileStats?) null);

        private IEnumerable<IFile> EnumerateEntries()
        {
            return Child;
        }
    }
}