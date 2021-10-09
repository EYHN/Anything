namespace Anything.FileSystem
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

        public FileEvent(EventType type, FileHandle fileHandle, FileStats stats)
        {
            Type = type;
            FileHandle = fileHandle;
            Stats = stats;
        }

        public EventType Type { get; }

        public FileHandle FileHandle { get; }

        public FileStats Stats { get; }

        public static FileEvent Created(FileHandle fileHandle, FileStats stats)
        {
            return new FileEvent(EventType.Created, fileHandle, stats);
        }

        public static FileEvent Deleted(FileHandle fileHandle, FileStats stats)
        {
            return new FileEvent(EventType.Deleted, fileHandle, stats);
        }

        public static FileEvent Changed(FileHandle fileHandle, FileStats stats)
        {
            return new FileEvent(EventType.Changed, fileHandle, stats);
        }
    }
}
