using System.IO;
using OwnHub.File.Base;

namespace OwnHub.File.Local
{
    public class FileSystem : BaseFileSystem
    {
        public static readonly FileSystem TestFilesystem = new FileSystem(System.IO.Directory.GetCurrentDirectory());
        private string RootPath;

        public FileSystem(string rootPath)
        {
            RootPath = Path.GetFullPath(rootPath);
        }

        public override IDirectory OpenDirectory(string path)
        {
            string realPath = GetRealPath(path);
            return new Directory(PathUtils.Resolve(path), new DirectoryInfo(realPath));
        }

        public override IFile Open(string path)
        {
            string realPath = GetRealPath(path);

            if (System.IO.Directory.Exists(realPath))
            {
                return new Directory(PathUtils.Resolve(path), new DirectoryInfo(realPath));
            }
            else if (System.IO.File.Exists(realPath))
            {
                return new RegularFile(PathUtils.Resolve(path), new FileInfo(realPath));
            }
            else
            {
                throw new FileNotFoundException();
            }
            
        }

        private string GetRealPath(string path)
        {
            return Path.Join(RootPath, PathUtils.Resolve(path));
        }
    }
}