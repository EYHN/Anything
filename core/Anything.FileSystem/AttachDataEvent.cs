using Anything.FileSystem.Tracker;

namespace Anything.FileSystem
{
    public class AttachDataEvent
    {
        /// <summary>
        ///     Type of file events.
        /// </summary>
        public enum EventType
        {
            /// <summary>
            ///     Event when file is deleted.
            /// </summary>
            Deleted
        }

        public AttachDataEvent(EventType type, FileHandle fileHandle, FileAttachedData attachedData)
        {
            Type = type;
            FileHandle = fileHandle;
            AttachedData = attachedData;
        }

        public EventType Type { get; }

        public FileHandle FileHandle { get; }

        public FileAttachedData AttachedData { get; }

        public static AttachDataEvent Deleted(FileHandle fileHandle, FileAttachedData attachedData)
        {
            return new AttachDataEvent(EventType.Deleted, fileHandle, attachedData);
        }
    }
}
