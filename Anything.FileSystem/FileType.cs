using System;

namespace Anything.FileSystem
{
    /// <summary>
    /// Enumeration of file types. The types File and Directory can also be a symbolic links,
    /// in that case use FileType.File | FileType.SymbolicLink and FileType.Directory | FileType.SymbolicLink.
    /// </summary>
    [Flags]
    public enum FileType
    {
        File = 1,
        Directory = 2,
        SymbolicLink = 64,
        Unknown = 0
    }
}
