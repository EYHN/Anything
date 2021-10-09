using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Microsoft.Extensions.FileProviders;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;
using NotSupportedException = Anything.FileSystem.Exception.NotSupportedException;

namespace Anything.FileSystem.Impl
{
    /// <summary>
    ///     File system provider, providing files from an <see cref="EmbeddedFileProvider" />.
    /// </summary>
    public class EmbeddedFileSystem : BaseStaticFileSystem
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedFileSystem" /> class.
        /// </summary>
        /// <param name="embeddedFileProvider">The embedded file provider associated with this.</param>
        public EmbeddedFileSystem(EmbeddedFileProvider embeddedFileProvider)
        {
            _embeddedFileProvider = embeddedFileProvider;
        }

        public override ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(string path)
        {
            throw new NotSupportedException("The embedded file system does not support read directory operation.");
        }

        public override async ValueTask<ReadOnlyMemory<byte>> ReadFile(string path)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(path);
            }

            if (fileInfo.IsDirectory)
            {
                throw new FileIsADirectoryException(path);
            }

            await using var memoryStream = new MemoryStream();
            await using (var readStream = fileInfo.CreateReadStream())
            {
                await readStream.CopyToAsync(memoryStream);
            }

            return memoryStream.ToArray();
        }

        public override ValueTask<FileStats> Stat(string path)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(path);
            }

            return ValueTask.FromResult(new FileStats(
                fileInfo.LastModified,
                fileInfo.LastModified,
                fileInfo.Length,
                FileType.File,
                new FileHash("(ignore)")));
        }

        public override async ValueTask<T> ReadFileStream<T>(string path, Func<Stream, ValueTask<T>> reader)
        {
            var fileInfo = _embeddedFileProvider.GetFileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(path);
            }

            if (fileInfo.IsDirectory)
            {
                throw new FileIsADirectoryException(path);
            }

            await using var stream = fileInfo.CreateReadStream();
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
