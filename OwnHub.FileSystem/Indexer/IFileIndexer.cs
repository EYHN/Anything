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
        /// Attach metadata to the path.
        /// </summary>
        /// <param name="path">The path to attach. This path must have been indexed.</param>
        /// <param name="metadata">The metadata to be attached.</param>
        /// <param name="replace">If the metadata with the same key already exists, replace it.</param>
        public ValueTask AttachMetadata(string path, Metadata metadata, bool replace = false);

        /// <summary>
        /// Delegation for handling file change events.
        /// If a file is recreated, the deleted event happen before the created event.
        /// </summary>
        /// <param name="events">The event list.</param>
        public delegate void ChangeEventHandler(ChangeEvent[] events);

        /// <summary>
        /// On file change event.
        /// </summary>
        public event ChangeEventHandler OnFileChange;

        /// <summary>
        /// Type of file change events.
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// Event when files are created.
            /// </summary>
            Created,

            /// <summary>
            /// Event when files are deleted.
            /// </summary>
            Deleted,

            /// <summary>
            /// Event when files are changed.
            /// </summary>
            Changed
        }

        public record ChangeEvent(EventType Type, string Path, Metadata[]? Metadata = null);

        /// <summary>
        /// The file metadata.
        /// </summary>
        /// <param name="Key">The key of the metadata. The key on the same file is unique.</param>
        /// <param name="Data">The data of the metadata.</param>
        public record Metadata(string Key, string? Data);
    }
}
