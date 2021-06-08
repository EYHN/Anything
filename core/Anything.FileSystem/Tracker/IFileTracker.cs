using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.FileSystem.Tracker
{
    /// <summary>
    ///     File tracker for tracking file system changes.
    /// </summary>
    public interface IFileTracker
    {
        /// <summary>
        ///     Delegation for handling file change events.
        ///     If a file is recreated, the deleted event happen before the created event.
        /// </summary>
        /// <param name="events">The event list.</param>
        public delegate void ChangeEventHandler(FileChangeEvent[] events);

        /// <summary>
        ///     Create indexes of the contents in the directory.
        /// </summary>
        /// <param name="url">The url of the directory.</param>
        /// <param name="contents">The contents in the directory.</param>
        public ValueTask IndexDirectory(Url url, (string Name, FileRecord Record)[] contents);

        /// <summary>
        ///     Create the index of the file.
        /// </summary>
        /// <param name="url">The url of the file.</param>
        /// <param name="record">The record of the file. Null means the file is deleted.</param>
        public ValueTask IndexFile(Url url, FileRecord? record);

        /// <summary>
        ///     Attach tag to the url.
        /// </summary>
        /// <param name="url">The url to attach. This url must have been indexed.</param>
        /// <param name="trackTag">The tag to be attached.</param>
        /// <param name="replace">If the tag with the same key already exists, replace it.</param>
        public ValueTask AttachTag(Url url, FileTrackTag trackTag, bool replace = false);

        /// <summary>
        ///     Get tags attached to the url.
        /// </summary>
        /// <param name="url">The url of tags.</param>
        public ValueTask<FileTrackTag[]> GetTags(Url url);

        /// <summary>
        ///     On file change event.
        /// </summary>
        public event ChangeEventHandler OnFileChange;
    }
}
