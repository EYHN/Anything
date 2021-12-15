using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem.Property;
using Anything.FileSystem.Walker;
using Anything.Utils;
using NotSupportedException = Anything.FileSystem.Exception.NotSupportedException;

namespace Anything.FileSystem.Singleton.Impl;

public abstract class BaseStaticFileSystem : BaseFileSystem
{
    private readonly Dictionary<string, ReadOnlyMemory<byte>?> _properties = new();

    protected override ValueTask<FileHandle> CreateFileHandle(string path)
    {
        return ValueTask.FromResult(SerializeFileHandle(path));
    }

    public override ValueTask<string?> GetRealPath(FileHandle fileHandle)
    {
        return ValueTask.FromResult<string?>(null);
    }

    public override ValueTask<string> GetFileName(FileHandle fileHandle)
    {
        return ValueTask.FromResult(PathLib.Basename(DeserializeFileHandle(fileHandle)));
    }

    protected override ValueTask<string> GetFilePath(FileHandle fileHandle)
    {
        return ValueTask.FromResult(DeserializeFileHandle(fileHandle));
    }

    public override ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name)
    {
        throw new NotSupportedException("The static file system does not support create directory operation.");
    }

    public override ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive)
    {
        throw new NotSupportedException("The static file system does not support delete operation.");
    }

    public override async ValueTask<IEnumerable<Dirent>> ReadDirectory(FileHandle fileHandle)
    {
        var directoryPath = DeserializeFileHandle(fileHandle);
        return (await ReadDirectory(directoryPath)).Select(entry =>
            new Dirent(entry.Name, SerializeFileHandle(PathLib.Join(directoryPath, entry.Name)), entry.Stats));
    }

    public override ValueTask<ReadOnlyMemory<byte>> ReadFile(FileHandle fileHandle)
    {
        return ReadFile(DeserializeFileHandle(fileHandle));
    }

    public override ValueTask<FileHandle> Rename(
        FileHandle fileHandle,
        FileHandle oldParentFileHandle,
        string oldName,
        FileHandle newParentFileHandle,
        string newName)
    {
        throw new NotSupportedException("The static file system does not support rename operation.");
    }

    public override ValueTask<FileStats> Stat(FileHandle fileHandle)
    {
        return Stat(DeserializeFileHandle(fileHandle));
    }

    public override ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content)
    {
        throw new NotSupportedException("The static file system does not support write file operation.");
    }

    public override ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content)
    {
        throw new NotSupportedException("The static file system does not support create file operation.");
    }

    public override ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader)
    {
        return ReadFileStream(DeserializeFileHandle(fileHandle), reader);
    }

    public override ValueTask SetProperty(
        FileHandle fileHandle,
        string name,
        ReadOnlyMemory<byte> value,
        PropertyFeature feature = PropertyFeature.None)
    {
        _properties[$"'{fileHandle.Identifier}':'{name}'"] = value;
        return ValueTask.CompletedTask;
    }

    public override ValueTask<ReadOnlyMemory<byte>?> GetProperty(FileHandle fileHandle, string name)
    {
        return ValueTask.FromResult(_properties.GetValueOrDefault($"'{fileHandle.Identifier}':'{name}'"));
    }

    public override ValueTask RemoveProperty(FileHandle fileHandle, string name)
    {
        _properties.Remove($"'{fileHandle.Identifier}':'{name}'");
        return ValueTask.CompletedTask;
    }

    public override IFileSystemWalker CreateWalker(FileHandle rootFileHandle)
    {
        return FileSystemWalkerFactory.CreateGenericWalker(this, rootFileHandle);
    }

    protected abstract ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(string path);

    protected abstract ValueTask<ReadOnlyMemory<byte>> ReadFile(string path);

    protected abstract ValueTask<FileStats> Stat(string path);

    protected abstract ValueTask<T> ReadFileStream<T>(string path, Func<Stream, ValueTask<T>> reader);

    private static string DeserializeFileHandle(FileHandle fileHandle)
    {
        return fileHandle.Identifier;
    }

    private static FileHandle SerializeFileHandle(string path)
    {
        return new FileHandle(path);
    }
}
