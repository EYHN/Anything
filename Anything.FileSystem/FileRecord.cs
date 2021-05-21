using System;

namespace Anything.FileSystem
{
    public record FileRecord(string IdentifierTag, string ContentTag, FileType Type, DateTimeOffset LastChangeTime);
}
