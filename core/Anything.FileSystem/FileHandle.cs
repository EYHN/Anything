using System;

namespace Anything.FileSystem;

/// <summary>
///     Similar to: <a href="https://man7.org/linux/man-pages/man2/open_by_handle_at.2.html">file_handle struct in linux</a>.
/// </summary>
public record FileHandle
{
    public FileHandle(string identifier, string? debugMessage = null)
    {
        Identifier = identifier;
        DebugMessage = debugMessage;
    }

    public string Identifier { get; }

    internal string? DebugMessage { get; }

    public virtual bool Equals(FileHandle? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Identifier == other.Identifier;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier);
    }

    public override string ToString()
    {
        return "FileHandle(" + Identifier + "," + DebugMessage + ")";
    }
}
