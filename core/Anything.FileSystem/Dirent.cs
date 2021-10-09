namespace Anything.FileSystem
{
    public record Dirent(string Name, FileHandle FileHandle, FileStats Stats);
}
