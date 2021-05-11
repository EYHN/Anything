using System;

namespace OwnHub.FileSystem
{
    public record FileRecord(string IdentifierTag, string ContentTag, FileType Type, DateTimeOffset LastChangeTime);
}
