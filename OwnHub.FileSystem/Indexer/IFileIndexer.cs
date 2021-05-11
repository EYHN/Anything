using System.Threading.Tasks;

namespace OwnHub.FileSystem.Indexer
{
    /// <summary>
    /// File indexer for tracking file system changes.
    /// </summary>
    public interface IFileIndexer
    {
        /// <summary>
        /// Create indexes of the contents in the directory.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="contents">The contents in the directory.</param>
        public ValueTask IndexDirectory(string path, (string Name, FileRecord Record)[] contents);

        /// <summary>
        /// Create the index of the file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="record">The record of the file. Null means the file is deleted.</param>
        public ValueTask IndexFile(string path, FileRecord? record);

        /// <summary>
        /// Delegation for handling file change events.
        /// </summary>
        /// <param name="event">The event data.</param>
        public delegate void FileChangeEventHandler(FileChangeEvent @event);

        /// <summary>
        /// File change event.
        /// </summary>
        public event FileChangeEventHandler OnFileChange;

        /// <summary>
        /// Data for file change events.
        /// It is possible for a file to be deleted and created at the same time,
        /// the deleted event happen before the created event.
        /// </summary>
        /// <param name="Created">The files that were created in this event.</param>
        /// <param name="Deleted">The files that were deleted in this event.</param>
        /// <param name="Changed">The files that were changed in this event.</param>
        public record FileChangeEvent(string[] Created, string[] Deleted, string[] Changed);
    }
}
