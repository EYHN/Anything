using System;

namespace OwnHub.FileSystem
{
    public record FileStat(DateTimeOffset CreationTime, DateTimeOffset LastWriteTime, long Size, FileType Type);
}
