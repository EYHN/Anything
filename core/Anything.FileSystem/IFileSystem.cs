﻿using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Provider;
using Anything.Utils;

namespace Anything.FileSystem
{
    public interface IFileSystem : IFileSystemProviderSupportStream
    {
        /// <summary>
        ///     Copy a file or directory.
        ///     Note that the copy operation may modify the modification and creation times, timestamp behavior depends on the implementation.
        /// </summary>
        /// <param name="source">The existing file location.</param>
        /// <param name="destination">The destination location.</param>
        /// <param name="overwrite">Overwrite existing files.</param>
        /// <exception cref="FileNotFoundException"><paramref name="source" /> or parent of <paramref name="destination" /> doesn't exist.</exception>
        /// <exception cref="FileExistsException">files exists and <paramref name="overwrite" /> is false.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public ValueTask Copy(Url source, Url destination, bool overwrite);

        /// <summary>
        ///     Convert url to local file path, if there is no local path, return null.
        /// </summary>
        /// <param name="url">the url to be converted.</param>
        public string? ToLocalPath(Url url);
    }
}