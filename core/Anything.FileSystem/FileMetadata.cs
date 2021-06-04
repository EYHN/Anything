namespace Anything.FileSystem
{
    /// <summary>
    ///     The file metadata.
    /// </summary>
    /// <param name="Key">The key of the metadata. The key on the same file is unique.</param>
    /// <param name="Data">The data of the metadata.</param>
    public record FileMetadata(string Key, string? Data = null);
}
