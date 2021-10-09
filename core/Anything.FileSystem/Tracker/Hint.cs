using System.Collections.Immutable;
using Anything.Utils;

namespace Anything.FileSystem.Tracker
{
    public abstract record Hint;

    public record FileHint(
        string Path,
        FileHandle FileHandle,
        FileStats FileStats) : Hint;

    public record DirectoryHint(
        string Path,
        FileHandle FileHandle,
        ImmutableArray<Dirent> Contents) : Hint;

    public record DeletedHint(string Path, FileHandle FileHandle) : Hint;

    public record AttachedDataHint(
        string Path,
        FileHandle FileHandle,
        FileStats FileStats,
        FileAttachedData AttachedData) : Hint;
}
