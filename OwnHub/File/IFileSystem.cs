namespace OwnHub.File
{
    public interface IFileSystem
    {
        public IDirectory OpenDirectory(string path);

        public IFile Open(string path);
    }
}