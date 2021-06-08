namespace Anything.FileSystem.Tracker
{
    /// <summary>
    ///     The file track tag.
    /// </summary>
    /// <param name="Key">The key of the tag. The key on the same file is unique.</param>
    /// <param name="Data">The data of the tag.</param>
    public record FileTrackTag(string Key, string? Data = null);
}
