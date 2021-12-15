namespace Anything.FileSystem;

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
        Changed,

        /// <summary>
        ///     Event when property is changed.
        /// </summary>
        PropertyUpdated
    }

    public FileEvent(EventType type, FileHandle fileHandle)
    {
        Type = type;
        FileHandle = fileHandle;
    }

    public EventType Type { get; }

    public FileHandle FileHandle { get; }

    public static FileEvent Created(FileHandle fileHandle)
    {
        return new FileEvent(EventType.Created, fileHandle);
    }

    public static FileEvent Deleted(FileHandle fileHandle)
    {
        return new FileEvent(EventType.Deleted, fileHandle);
    }

    public static FileEvent Changed(FileHandle fileHandle)
    {
        return new FileEvent(EventType.Changed, fileHandle);
    }
}
