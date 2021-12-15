using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Property;
using Anything.FileSystem.Walker;
using Anything.Utils;

namespace Anything.FileSystem.Singleton.Impl;

public abstract class BaseFileSystem : ISingletonFileSystem
{
    public ValueTask<FileHandle> CreateFileHandle(Url url)
    {
        return CreateFileHandle(url.Path);
    }

    public abstract ValueTask<string?> GetRealPath(FileHandle fileHandle);

    public abstract ValueTask<string> GetFileName(FileHandle fileHandle);

    public abstract IFileSystemWalker CreateWalker(FileHandle rootFileHandle);

    public async ValueTask<Url> GetUrl(FileHandle fileHandle)
    {
        return new Url("unknown", "unknown", await GetFilePath(fileHandle));
    }

    public abstract ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name);

    public abstract ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive);

    public abstract ValueTask<IEnumerable<Dirent>> ReadDirectory(FileHandle fileHandle);

    public abstract ValueTask<ReadOnlyMemory<byte>> ReadFile(FileHandle fileHandle);

    public abstract ValueTask<FileHandle> Rename(
        FileHandle fileHandle,
        FileHandle oldParentFileHandle,
        string oldName,
        FileHandle newParentFileHandle,
        string newName);

    public abstract ValueTask<FileStats> Stat(FileHandle fileHandle);

    public abstract ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content);

    public abstract ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content);

    public abstract ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader);

    public abstract ValueTask SetProperty(
        FileHandle fileHandle,
        string name,
        ReadOnlyMemory<byte> value,
        PropertyFeature feature = PropertyFeature.None);

    public abstract ValueTask<ReadOnlyMemory<byte>?> GetProperty(FileHandle fileHandle, string name);

    public abstract ValueTask RemoveProperty(FileHandle fileHandle, string name);

    protected abstract ValueTask<FileHandle> CreateFileHandle(string path);

    protected abstract ValueTask<string> GetFilePath(FileHandle fileHandle);
}
