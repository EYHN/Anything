using System;

namespace StagingBox.FileSystem
{
    public record FileRecord(string IdentifierTag, string ContentTag, FileType Type, DateTimeOffset LastChangeTime);
}
