using System;
using StagingBox.FileSystem.Indexer;
using StagingBox.Utils;

namespace StagingBox.FileSystem
{
    public record FileChangeEvent
    {
        public EventType Type { get; init; }

        public Url Url { get; init; }

        public FileMetadata[] Metadata { get; init; }

        public FileChangeEvent(EventType type, Url url, FileMetadata[]? metadata = null)
        {
            Type = type;
            Url = url;
            Metadata = metadata ?? Array.Empty<FileMetadata>();
        }

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
    }
}
