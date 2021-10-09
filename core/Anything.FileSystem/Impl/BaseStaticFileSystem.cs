using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Anything.Utils.Event;
using NotSupportedException = Anything.FileSystem.Exception.NotSupportedException;

namespace Anything.FileSystem.Impl
{
    public abstract class BaseStaticFileSystem : IFileSystem
    {
        public abstract ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(string path);

        public abstract ValueTask<ReadOnlyMemory<byte>> ReadFile(string path);

        public abstract ValueTask<FileStats> Stat(string path);

        public abstract ValueTask<T> ReadFileStream<T>(string path, Func<Stream, ValueTask<T>> reader);

        public ValueTask<FileHandle> CreateFileHandle(string path)
        {
            return ValueTask.FromResult(SerializeFileHandle(path));
        }

        public ValueTask<string?> GetRealPath(FileHandle fileHandle)
        {
            return ValueTask.FromResult<string?>(null);
        }

        public ValueTask<string> GetFileName(FileHandle fileHandle)
        {
            return ValueTask.FromResult(PathLib.Basename(DeserializeFileHandle(fileHandle)));
        }

        public ValueTask<string> GetFilePath(FileHandle fileHandle)
        {
            return ValueTask.FromResult(DeserializeFileHandle(fileHandle));
        }

        public ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name)
        {
            throw new NotSupportedException("The static file system does not support create directory operation.");
        }

        public ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive)
        {
            throw new NotSupportedException("The static file system does not support delete operation.");
        }

        public async ValueTask<IEnumerable<Dirent>> ReadDirectory(FileHandle fileHandle)
        {
            var directoryPath = DeserializeFileHandle(fileHandle);
            return (await ReadDirectory(directoryPath)).Select(entry =>
                new Dirent(entry.Name, SerializeFileHandle(PathLib.Join(directoryPath, entry.Name)), entry.Stats));
        }

        public ValueTask<ReadOnlyMemory<byte>> ReadFile(FileHandle fileHandle)
        {
            return ReadFile(DeserializeFileHandle(fileHandle));
        }

        public ValueTask<FileHandle> Rename(
            FileHandle fileHandle,
            FileHandle oldParentFileHandle,
            string oldName,
            FileHandle newParentFileHandle,
            string newName)
        {
            throw new NotSupportedException("The static file system does not support rename operation.");
        }

        public ValueTask<FileStats> Stat(FileHandle fileHandle)
        {
            return Stat(DeserializeFileHandle(fileHandle));
        }

        public ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content)
        {
            throw new NotSupportedException("The static file system does not support write file operation.");
        }

        public ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content)
        {
            throw new NotSupportedException("The static file system does not support create file operation.");
        }

        public ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader)
        {
            return ReadFileStream(DeserializeFileHandle(fileHandle), reader);
        }

        public ValueTask AttachData(FileHandle fileHandle, FileAttachedData attachedData)
        {
            throw new NotSupportedException("The static file system does not support attach data operation.");
        }

        public Event<FileEvent[]> FileEvent => Event<FileEvent[]>.Silent;

        public Event<AttachDataEvent[]> AttachDataEvent => Event<AttachDataEvent[]>.Silent;

        public ValueTask WaitComplete()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask WaitFullScan()
        {
            return ValueTask.CompletedTask;
        }

        public IFileSystemWalker CreateWalker(FileHandle rootFileHandle)
        {
            return FileSystemWalkerFactory.CreateGenericWalker(this, rootFileHandle);
        }

        private static string DeserializeFileHandle(FileHandle fileHandle)
        {
            return fileHandle.Identifier;
        }

        private static FileHandle SerializeFileHandle(string path)
        {
            return new(path);
        }
    }
}
