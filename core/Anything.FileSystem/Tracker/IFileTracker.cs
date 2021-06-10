using System.Threading.Tasks;
using Anything.Utils;
using Anything.Utils.Event;

namespace Anything.FileSystem.Tracker
{
    /// <summary>
    ///     File tracker for tracking file system changes.
    /// </summary>
    public interface IFileTracker
    {
        /// <summary>
        ///     Gets the file change event.
        /// </summary>
        public Event<FileChangeEvent[]> OnFileChange { get; }

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
    }
}
