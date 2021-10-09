namespace Anything.FileSystem
{
    /// <summary>
    /// Similar to: <a href="https://man7.org/linux/man-pages/man2/open_by_handle_at.2.html">file_handle struct in linux</a>.
    /// </summary>
    public record FileHandle
    {
        public string Identifier { get; }

        internal string? DebugMessage { get; }

        public FileHandle(string identifier, string? debugMessage = null)
        {
            Identifier = identifier;
            DebugMessage = debugMessage;
        }
    }
}
