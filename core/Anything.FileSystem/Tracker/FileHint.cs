using Anything.Utils;

namespace Anything.FileSystem.Tracker
{
    /// <summary>
    ///     A file hint provided by the file system to the file tracker.
    /// </summary>
    /// <param name="Url">The hint url.</param>
    /// <param name="FileRecord">File records associated with this url.</param>
    public record FileHint(
        Url Url,
        FileRecord FileRecord);

    /// <summary>
    ///     A directory hint provided by the file system to the file tracker.
    /// </summary>
    /// <param name="Url">The hint url.</param>
    /// <param name="Contents">Directory contents associated with this url.</param>
    public record DirectoryHint(
        Url Url,
        (string Name, FileRecord Record)[] Contents);

    /// <summary>
    ///     A deleted file hint provided by the file system to the file tracker.
    /// </summary>
    /// <param name="Url">The hint url.</param>
    public record DeletedHint(Url Url);
}
