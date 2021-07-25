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
        ///     Gets the file event.
        /// </summary>
        public Event<FileEvent[]> FileEvent { get; }

        /// <summary>
        ///     Attach data to the url.
        /// </summary>
        /// <param name="url">The url to attach. This url must have been indexed.</param>
        /// <param name="fileRecord">File records associated with this url.</param>
        /// <param name="data">The data to be attached.</param>
        public ValueTask AttachData(Url url, FileRecord fileRecord, FileAttachedData data);
    }
}
