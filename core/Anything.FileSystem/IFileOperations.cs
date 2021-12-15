using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.FileSystem;

public interface IFileOperations
{
    public ValueTask<FileHandle> CreateFileHandle(Url url);

    public ValueTask<string?> GetRealPath(FileHandle fileHandle);

    public ValueTask<string> GetFileName(FileHandle fileHandle);

    public ValueTask<Url> GetUrl(FileHandle fileHandle);

    public ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name);

    public ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive);

    public ValueTask<IEnumerable<Dirent>> ReadDirectory(FileHandle fileHandle);

    public ValueTask<ReadOnlyMemory<byte>> ReadFile(FileHandle fileHandle);

    public ValueTask<FileHandle> Rename(
        FileHandle fileHandle,
        FileHandle oldParentFileHandle,
        string oldName,
        FileHandle newParentFileHandle,
        string newName);

    public ValueTask<FileStats> Stat(FileHandle fileHandle);

    public ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content);

    public ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content);

    public ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader);
}
