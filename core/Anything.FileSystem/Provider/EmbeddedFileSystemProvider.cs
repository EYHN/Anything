using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem.Provider
{
    /// <summary>
    ///     File system provider, providing files from an <see cref="EmbeddedFileProvider" />.
    /// </summary>
    public class EmbeddedFileSystemProvider : IFileSystemProviderSupportStream
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedFileSystemProvider" /> class.
        /// </summary>
        /// <param name="embeddedFileProvider">The embedded file provider associated with this.</param>
        public EmbeddedFileSystemProvider(EmbeddedFileProvider embeddedFileProvider)
        {
            _embeddedFileProvider = embeddedFileProvider;
        }

        /// <inheritdoc />
        public ValueTask CreateDirectory(Url url)
        {
            throw new NoPermissionsException(url);
        }

        /// <inheritdoc />
        public ValueTask Delete(Url url, bool recursive)
        {
            throw new NoPermissionsException(url);
        }

        /// <inheritdoc />
        public ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            throw new NoPermissionsException(url);
        }

        /// <inheritdoc />
        public async ValueTask<byte[]> ReadFile(Url url)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(url.Path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(url);
            }

            if (fileInfo.IsDirectory)
            {
                throw new FileIsADirectoryException(url);
            }

            await using var memoryStream = new MemoryStream();
            await fileInfo.CreateReadStream().CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <inheritdoc />
        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            throw new NoPermissionsException(oldUrl);
        }

        /// <inheritdoc />
        public ValueTask<FileStats> Stat(Url url)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(url.Path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(url);
            }

            return ValueTask.FromResult(new FileStats(fileInfo.LastModified, fileInfo.LastModified, fileInfo.Length, FileType.File));
        }

        /// <inheritdoc />
        public ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            throw new NoPermissionsException(url);
        }

        public async ValueTask<T> ReadFileStream<T>(Url url, Func<Stream, ValueTask<T>> reader)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(url.Path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(url);
            }

            if (fileInfo.IsDirectory)
            {
                throw new FileIsADirectoryException(url);
            }

            var stream = fileInfo.CreateReadStream();
            T result;
            try
            {
                result = await reader(stream);
            }
            catch (System.Exception e)
            {
                throw new AggregateException("Exception from reader", e);
            }

            return result;
        }
    }
}
