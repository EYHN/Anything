using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.Utils;

namespace Anything.FileSystem.Provider
{
    public interface IFileSystemProviderSupportStream : IFileSystemProvider
    {
        /// <summary>
        ///     Open a stream for reading files.
        /// </summary>
        /// <param name="url">The uri of the file.</param>
        /// <exception cref="Exception.FileNotFoundException"><paramref name="url" /> doesn't exist.</exception>
        /// <exception cref="FileIsADirectoryException"><paramref name="url" /> is a directory.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        /// <returns>A task that resolves an stream.</returns>
        public ValueTask<Stream> OpenReadFileStream(Url url);
    }
}
