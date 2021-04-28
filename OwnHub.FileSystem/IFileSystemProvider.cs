using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;

namespace OwnHub.FileSystem
{
    public interface IFileSystemProvider
    {
        /// <summary>
        /// Copy a file or directory.
        /// Note that the copy operation may modify the modification and creation times, timestamp behavior depends on the implementation.
        /// </summary>
        /// <param name="source">The existing file location.</param>
        /// <param name="destination">The destination location.</param>
        /// <param name="overwrite">Overwrite existing files.</param>
        /// <exception cref="FileNotFoundException"><paramref name="source"/> or parent of <paramref name="destination"/> doesn't exist.</exception>
        /// <exception cref="FileExistsException">files exists and <paramref name="overwrite"/> is false.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        // public ValueTask Copy(Uri source, Uri destination, bool overwrite);

        /// <summary>
        /// Create a new directory.
        /// </summary>
        /// <param name="uri">The uri of the new directory.</param>
        /// <exception cref="FileNotFoundException">parent of <paramref name="uri"/> doesn't exist.</exception>
        /// <exception cref="FileExistsException">when <paramref name="uri"/> already exists.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public ValueTask CreateDirectory(Uri uri);

        /// <summary>
        /// Removes a file or directory.
        /// </summary>
        /// <param name="uri">The uri of the file or directory that is to be deleted.</param>
        /// <param name="recursive">Remove directories and their contents recursively.</param>
        /// <exception cref="FileNotFoundException"><paramref name="uri"/> doesn't exist.</exception>
        /// <exception cref="FileIsADirectoryException"><paramref name="uri"/> is a directory and <paramref name="recursive"/> is false.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public ValueTask Delete(Uri uri, bool recursive);

        /// <summary>
        /// Retrieve all entries of a directory.
        /// </summary>
        /// <param name="uri">The uri of the directory.</param>
        /// <exception cref="FileNotFoundException"><paramref name="uri"/> doesn't exist.</exception>
        /// <exception cref="FileNotADirectoryException"><paramref name="uri"/> is not a directory.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        /// <returns>A task that resolves a collection of name/type pair.</returns>
        public ValueTask<IEnumerable<KeyValuePair<string, FileType>>> ReadDirectory(Uri uri);

        /// <summary>
        /// Read the entire contents of a file.
        /// </summary>
        /// <param name="uri">The uri of the file.</param>
        /// <exception cref="FileNotFoundException"><paramref name="uri"/> doesn't exist.</exception>
        /// <exception cref="FileIsADirectoryException"><paramref name="uri"/> is a directory.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        /// <returns>A task that resolves an array of bytes.</returns>
        public ValueTask<byte[]> ReadFile(Uri uri);

        /// <summary>
        /// Copy a file or directory. The directory can have contents.
        /// Note that the rename operation may modify the modification and creation times, timestamp behavior depends on the implementation.
        /// </summary>
        /// <param name="oldUri">The existing file.</param>
        /// <param name="newUri">The new location.</param>
        /// <param name="overwrite">Overwrite existing files.</param>
        /// <exception cref="FileNotFoundException"><paramref name="oldUri"/> or parent of <paramref name="newUri"/> doesn't exist.</exception>
        /// <exception cref="FileExistsException"><paramref name="newUri"/> exists and <paramref name="overwrite"/> is false.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public ValueTask Rename(Uri oldUri, Uri newUri, bool overwrite);

        /// <summary>
        /// Retrieve stats about a file or directory.
        /// Note that the stats for symbolic links should be the stats of the file they refer to.
        /// </summary>
        /// <param name="uri">The uri of the file or directory to retrieve metadata about.</param>
        /// <exception cref="FileNotFoundException"><paramref name="uri"/> doesn't exist.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public ValueTask<FileStat> Stat(Uri uri);

        /// <summary>
        /// Write data to a file, replacing its entire contents.
        /// </summary>
        /// <param name="uri">The uri of the file.</param>
        /// <param name="content">The new content of the file.</param>
        /// <param name="create">Create when the file does not exist.</param>
        /// <param name="overwrite">When the file exists, overwrite its entire contents.</param>
        /// <exception cref="FileNotFoundException"><paramref name="uri"/> doesn't exist and <paramref name="create"/> is false.</exception>
        /// <exception cref="FileExistsException"><paramref name="uri"/> exists and <paramref name="overwrite"/> is false.</exception>
        /// <exception cref="FileIsADirectoryException"><paramref name="uri"/> is a directory.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public ValueTask WriteFile(Uri uri, byte[] content, bool create = true, bool overwrite = true);
    }
}
