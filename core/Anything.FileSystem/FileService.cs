using Anything.FileSystem.Tracker;

namespace Anything.FileSystem
{
    public class FileService : IFileService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileService" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="fileTracker">The file tracker.</param>
        public FileService(IFileSystem fileSystem, IFileTracker fileTracker)
        {
            FileSystem = fileSystem;
            FileTracker = fileTracker;
        }

        /// <inheritdoc />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc />
        public IFileTracker FileTracker { get; }
    }
}
