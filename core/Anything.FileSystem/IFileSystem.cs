using System.Threading.Tasks;
using Anything.FileSystem.Walker;
using Anything.Utils.Event;

namespace Anything.FileSystem
{
    public interface IFileSystem : IFileOperations
    {
        public ValueTask<FileHandle> CreateFileHandle(string path);

        public ValueTask<string?> GetRealPath(FileHandle fileHandle);

        public ValueTask<string> GetFileName(FileHandle fileHandle);

        public ValueTask<string> GetFilePath(FileHandle fileHandle);

        #region Events

        /// <summary>
        ///     Gets the file event.
        /// </summary>
        public Event<FileEvent[]> FileEvent { get; }

        public Event<AttachDataEvent[]> AttachDataEvent { get; }

        #endregion

        /// <summary>
        ///     Test only. Wait for all pending tasks to be completed.
        /// </summary>
        public ValueTask WaitComplete();

        /// <summary>
        ///     Test only. Wait for a full scan to be completed.
        /// </summary>
        public ValueTask WaitFullScan();

        public IFileSystemWalker CreateWalker(FileHandle rootFileHandle);
    }
}
