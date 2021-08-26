using System;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.Utils;

namespace Anything.FileSystem.Provider
{
    public interface IFileSystemProviderSupportStream : IFileSystemProvider
    {
        /// <summary>
        ///     Open a stream for the file and call the reader.
        /// </summary>
        /// <param name="url">The uri of the file.</param>
        /// <param name="reader">The reader.</param>
        /// <exception cref="FileSystem.Exception.FileNotFoundException"><paramref name="url" /> doesn't exist.</exception>
        /// <exception cref="FileIsADirectoryException"><paramref name="url" /> is a directory.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        /// <exception cref="System.AggregateException">Exception from reader.</exception>
        public ValueTask ReadFileStream(Url url, Func<Stream, ValueTask> reader);

        /// <summary>
        ///     Open a stream for the file and call the reader, then return the result of the reader.
        /// </summary>
        /// <param name="url">The uri of the file.</param>
        /// <param name="reader">The reader. The return value will be used as the return value of this method.</param>
        /// <exception cref="FileSystem.Exception.FileNotFoundException"><paramref name="url" /> doesn't exist.</exception>
        /// <exception cref="FileIsADirectoryException"><paramref name="url" /> is a directory.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        /// <exception cref="System.AggregateException">Exception from reader.</exception>
        /// <returns>A task that resolves the result of the reader.</returns>
        public ValueTask<T> ReadFileStream<T>(Url url, Func<Stream, ValueTask<T>> reader);
    }
}
