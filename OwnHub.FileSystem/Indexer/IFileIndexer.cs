using System;
using System.Threading.Tasks;
using OwnHub.Utils;

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
        /// <param name="url">The url of the directory.</param>
        /// <param name="contents">The contents in the directory.</param>
        public ValueTask IndexDirectory(Url url, (string Name, FileRecord Record)[] contents);

        /// <summary>
        /// Create the index of the file.
        /// </summary>
        /// <param name="url">The url of the file.</param>
        /// <param name="record">The record of the file. Null means the file is deleted.</param>
        public ValueTask IndexFile(Url url, FileRecord? record);

        /// <summary>
        /// Attach metadata to the url.
        /// </summary>
        /// <param name="url">The url to attach. This url must have been indexed.</param>
        /// <param name="metadata">The metadata to be attached.</param>
        /// <param name="replace">If the metadata with the same key already exists, replace it.</param>
        public ValueTask AttachMetadata(Url url, FileMetadata metadata, bool replace = false);

        /// <summary>
        /// Get metadata attached to the url.
        /// </summary>
        /// <param name="url">The url of the metadata.</param>
        public ValueTask<FileMetadata[]> GetMetadata(Url url);

        /// <summary>
        /// Delegation for handling file change events.
        /// If a file is recreated, the deleted event happen before the created event.
        /// </summary>
        /// <param name="events">The event list.</param>
        public delegate void ChangeEventHandler(FileChangeEvent[] events);

        /// <summary>
        /// On file change event.
        /// </summary>
        public event ChangeEventHandler OnFileChange;
    }
}
