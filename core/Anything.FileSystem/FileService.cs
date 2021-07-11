using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Tracker;
using Anything.Utils;
using Anything.Utils.Event;

namespace Anything.FileSystem
{
    public class FileService : IFileService
    {
        private readonly IFileSystem _fileSystem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileService" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public FileService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ValueTask CreateDirectory(Url url)
        {
            return _fileSystem.CreateDirectory(url);
        }

        public ValueTask Delete(Url url, bool recursive)
        {
            return _fileSystem.Delete(url, recursive);
        }

        public ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            return _fileSystem.ReadDirectory(url);
        }

        public ValueTask<byte[]> ReadFile(Url url)
        {
            return _fileSystem.ReadFile(url);
        }

        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            return _fileSystem.Rename(oldUrl, newUrl, overwrite);
        }

        public ValueTask<FileStats> Stat(Url url)
        {
            return _fileSystem.Stat(url);
        }

        public ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            return _fileSystem.WriteFile(url, content, create, overwrite);
        }

        public ValueTask<Stream> OpenReadFileStream(Url url)
        {
            return _fileSystem.OpenReadFileStream(url);
        }

        public Event<FileEvent[]> FileEvent => _fileSystem.FileEvent;

        public Task AttachData(Url url, FileRecord fileRecord, FileAttachedData data)
        {
            return _fileSystem.AttachData(url, fileRecord, data);
        }

        public ValueTask WaitComplete()
        {
            return _fileSystem.WaitComplete();
        }

        public ValueTask Copy(Url source, Url destination, bool overwrite)
        {
            return _fileSystem.Copy(source, destination, overwrite);
        }

        public string? ToLocalPath(Url url)
        {
            return _fileSystem.ToLocalPath(url);
        }
    }
}
