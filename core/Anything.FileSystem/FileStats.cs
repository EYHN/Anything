using System;

namespace Anything.FileSystem;

public record FileStats(
    DateTimeOffset CreationTime,
    DateTimeOffset LastWriteTime,
    long Size,
    FileType Type,
    FileHash Hash);
