using OwnHub.File.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File.Local
{
    public class FileSystem : BaseFileSystem
    {
        public string RootPath;
        public static FileSystem _test_filesystem = new FileSystem(System.IO.Directory.GetCurrentDirectory());
        public FileSystem(string rootpath)
        {
            this.RootPath = Path.GetFullPath(rootpath);
        }

        public override IDirectory OpenDirectory(string path)
        {
            string realPath = GetRealPath(path);
            return new Directory(PathUtils.resolve(path), new DirectoryInfo(realPath));
        }

        public override IFile Open(string path)
        {
            string realPath = GetRealPath(path);
            return new RegularFile(PathUtils.resolve(path), new FileInfo(realPath));
        }

        string GetRealPath(string path)
        {
            return Path.Join(this.RootPath, PathUtils.resolve(path));
        }
    }
}
