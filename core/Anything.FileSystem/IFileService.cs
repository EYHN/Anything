using Anything.FileSystem.Tracker;

namespace Anything.FileSystem
{
    public interface IFileService
    {
        public IFileSystem FileSystem { get; }

        public IFileTracker FileTracker { get; }
    }
}
