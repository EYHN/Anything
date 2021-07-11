using System;
using Anything.Utils;

namespace Anything.FileSystem.Tracker
{
    public record FileEvent
    {
        /// <summary>
        ///     Type of file events.
        /// </summary>
        public enum EventType
        {
            /// <summary>
            ///     Event when file is created.
            /// </summary>
            Created,

            /// <summary>
            ///     Event when file is deleted.
            /// </summary>
            Deleted,

            /// <summary>
            ///     Event when file is changed.
            /// </summary>
            Changed
        }

        public FileEvent(EventType type, Url url, FileAttachedData[]? attachedData = null)
        {
            Type = type;
            Url = url;
            AttachedData = attachedData ?? Array.Empty<FileAttachedData>();
        }

        public EventType Type { get; }

        public Url Url { get; }

        public FileAttachedData[] AttachedData { get; }
    }
}
