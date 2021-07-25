using System.Collections.Generic;
using Anything.Utils;

namespace Anything.FileSystem.Walker
{
    /// <summary>
    ///     File system walker can enumerate all files on the file system.
    ///     Walker has a root url that controls the scope of the enumeration.
    ///     Please note that the files enumerated by the enumerator may have been modified or deleted by other threads.
    ///     In order not to miss changes to the file system by other threads, you should listen to
    ///     <see cref="Anything.FileSystem.Tracker.IFileTracker.FileEvent" /> of the file
    ///     system before opening the file walker.
    /// </summary>
    public interface IFileSystemWalker : IAsyncEnumerable<Url>
    {
        /// <summary>
        ///     Gets the root url of the walker.
        /// </summary>
        public Url RootUrl { get; }
    }
}
