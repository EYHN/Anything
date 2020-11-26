namespace OwnHub.File.Base
{
    public abstract class BaseFileSystem : IFileSystem
    {
        public abstract IDirectory OpenDirectory(string path);

        public abstract IFile Open(string path);
    }
}