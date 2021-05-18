using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using StagingBox.FileSystem.Exception;
using StagingBox.Utils;
using FileNotFoundException = StagingBox.FileSystem.Exception.FileNotFoundException;

namespace StagingBox.FileSystem.Provider
{
    /// <summary>
    /// File system provider, providing files from an <see cref="EmbeddedFileProvider"/>.
    /// </summary>
    public class EmbeddedFileSystemProvider : IFileSystemProviderSupportStream
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedFileSystemProvider"/> class.
        /// </summary>
        /// <param name="embeddedFileProvider">The embedded file provider associated with this.</param>
        public EmbeddedFileSystemProvider(EmbeddedFileProvider embeddedFileProvider)
        {
            _embeddedFileProvider = embeddedFileProvider;
        }

        /// <inheritdoc/>
        public ValueTask CreateDirectory(Url url)
        {
            throw new NoPermissionsException(url);
        }

        /// <inheritdoc/>
        public ValueTask Delete(Url url, bool recursive)
        {
            throw new NoPermissionsException(url);
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            throw new NoPermissionsException(url);
        }

        /// <inheritdoc/>
        public async ValueTask<byte[]> ReadFile(Url url)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(url.Path);
            if (!fileInfo.Exists)
            {
                throw new Exception.FileNotFoundException(url);
            }

            if (fileInfo.IsDirectory)
            {
                throw new FileIsADirectoryException(url);
            }

            await using var memoryStream = new MemoryStream();
            await fileInfo.CreateReadStream().CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <inheritdoc/>
        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            throw new NoPermissionsException(oldUrl);
        }

        /// <inheritdoc/>
        public ValueTask<FileStats> Stat(Url url)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(url.Path);
            if (!fileInfo.Exists)
            {
                throw new Exception.FileNotFoundException(url);
            }

            return ValueTask.FromResult(new FileStats(fileInfo.LastModified, fileInfo.LastModified, fileInfo.Length, FileType.File));
        }

        /// <inheritdoc/>
        public ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            throw new NoPermissionsException(url);
        }

        public ValueTask<Stream> OpenReadFileStream(Url url)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(url.Path);
            if (!fileInfo.Exists)
            {
                throw new Exception.FileNotFoundException(url);
            }

            if (fileInfo.IsDirectory)
            {
                throw new FileIsADirectoryException(url);
            }

            return ValueTask.FromResult(fileInfo.CreateReadStream());
        }
    }
}
