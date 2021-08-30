using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Anything.Utils.Event;

namespace Anything.FileSystem.Impl
{
    public abstract class WrappedFileSystem : Disposable, IFileSystem
    {
        protected abstract IFileSystem InnerFileSystem { get; }

        /// <inheritdoc />
        public ValueTask CreateDirectory(Url url)
        {
            return InnerFileSystem.CreateDirectory(url);
        }

        /// <inheritdoc />
        public ValueTask Delete(Url url, bool recursive)
        {
            return InnerFileSystem.Delete(url, recursive);
        }

        /// <inheritdoc />
        public ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            return InnerFileSystem.ReadDirectory(url);
        }

        /// <inheritdoc />
        public ValueTask<byte[]> ReadFile(Url url)
        {
            return InnerFileSystem.ReadFile(url);
        }

        /// <inheritdoc />
        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            return InnerFileSystem.Rename(oldUrl, newUrl, overwrite);
        }

        /// <inheritdoc />
        public ValueTask<FileStats> Stat(Url url)
        {
            return InnerFileSystem.Stat(url);
        }

        /// <inheritdoc />
        public ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            return InnerFileSystem.WriteFile(url, content, create, overwrite);
        }

        /// <inheritdoc />
        public ValueTask<T> ReadFileStream<T>(Url url, Func<Stream, ValueTask<T>> reader)
        {
            return InnerFileSystem.ReadFileStream(url, reader);
        }

        /// <inheritdoc />
        public Event<FileEvent[]> FileEvent => InnerFileSystem.FileEvent;

        /// <inheritdoc />
        public ValueTask AttachData(Url url, FileRecord fileRecord, FileAttachedData data)
        {
            return InnerFileSystem.AttachData(url, fileRecord, data);
        }

        /// <inheritdoc />
        public ValueTask Copy(Url source, Url destination, bool overwrite)
        {
            return InnerFileSystem.Copy(source, destination, overwrite);
        }

        /// <inheritdoc />
        public string? ToLocalPath(Url url)
        {
            return InnerFileSystem.ToLocalPath(url);
        }

        /// <inheritdoc />
        public ValueTask WaitComplete()
        {
            return InnerFileSystem.WaitComplete();
        }

        /// <inheritdoc />
        public ValueTask WaitFullScan()
        {
            return InnerFileSystem.WaitFullScan();
        }

        /// <inheritdoc />
        public IFileSystemWalker CreateWalker(Url rootUrl)
        {
            return InnerFileSystem.CreateWalker(rootUrl);
        }
    }
}
