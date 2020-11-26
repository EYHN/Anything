using System.IO;
using OwnHub.File.Base;

namespace OwnHub.File.Local
{
    public class FileSystem : BaseFileSystem
    {
        public static FileSystem TestFilesystem = new FileSystem(System.IO.Directory.GetCurrentDirectory());
        public string RootPath;

        public FileSystem(string rootpath)
        {
            RootPath = Path.GetFullPath(rootpath);
        }

        public override IDirectory OpenDirectory(string path)
        {
            string realPath = GetRealPath(path);
            return new Directory(PathUtils.Resolve(path), new DirectoryInfo(realPath));
        }

        public override IFile Open(string path)
        {
            string realPath = GetRealPath(path);
            return new RegularFile(PathUtils.Resolve(path), new FileInfo(realPath));
        }

        private string GetRealPath(string path)
        {
            return Path.Join(RootPath, PathUtils.Resolve(path));
        }
    }
}