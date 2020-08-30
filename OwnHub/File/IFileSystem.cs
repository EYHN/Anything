using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public interface IFileSystem
    {
        public IDirectory OpenDirectory(string path);

        public IFile Open(string path);
    }
}
