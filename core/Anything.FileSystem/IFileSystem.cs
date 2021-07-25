using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;

namespace Anything.FileSystem
{
    public interface IFileSystem : IFileSystemProviderSupportStream, IFileTracker
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

        /// <summary>
        ///     Test only. Wait for all pending tasks to be completed.
        /// </summary>
        public ValueTask WaitComplete();

        /// <summary>
        ///     Test only. Wait for a full scan to be completed.
        /// </summary>
        public ValueTask WaitFullScan();

        /// <summary>
        ///     Create a file walker at the given root url.
        /// </summary>
        /// <param name="rootUrl">The file walker of the file walker.</param>
        /// <returns>The file walker.</returns>
        public IFileSystemWalker CreateWalker(Url rootUrl);
    }
}
