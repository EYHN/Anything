using System;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.Utils;

namespace Anything.FileSystem.Extensions;

public static class FileServiceExtensions
{
    public static async ValueTask<FileHandle> Mkdirp(this IFileService fs, Url url)
    {
        var pathParts = PathLib.Split(url.Path);

        var currentUrl = url with { Path = "/" };
        var currentFileHandle = await fs.CreateFileHandle(currentUrl);
        foreach (var part in pathParts)
        {
            currentUrl = currentUrl with { Path = currentUrl.Path == "/" ? currentUrl.Path + part : currentUrl.Path + '/' + part };

            try
            {
                currentFileHandle = await fs.CreateDirectory(currentFileHandle, part);
            }
            catch (FileExistsException)
            {
                currentFileHandle = await fs.CreateFileHandle(currentUrl);
                if (!(await fs.Stat(currentFileHandle)).Type.HasFlag(FileType.Directory))
                {
                    throw new FileNotADirectoryException(currentUrl);
                }
            }
        }

        return currentFileHandle;
    }

    public static async ValueTask<FileHandle> CopyFile(
        this IFileService fs,
        FileHandle fileHandle,
        FileHandle parentFileHandle,
        string name)
    {
        var fileContent = await fs.ReadFile(fileHandle);
        return await fs.CreateFile(parentFileHandle, name, fileContent);
    }

    public static async ValueTask<FileHandle> CopyDirectory(
        this IFileService fs,
        FileHandle fileHandle,
        FileHandle parentFileHandle,
        string name)
    {
        var dirents = await fs.ReadDirectory(fileHandle);
        var newDirectoryFileHandle = await fs.CreateDirectory(parentFileHandle, name);
        foreach (var dirent in dirents)
        {
            if (dirent.Stats.Type.HasFlag(FileType.SymbolicLink))
            {
                throw new NotImplementedException();
            }

            if (dirent.Stats.Type.HasFlag(FileType.Directory))
            {
                await fs.CopyDirectory(dirent.FileHandle, newDirectoryFileHandle, dirent.Name);
            }
            else
            {
                await fs.CopyFile(dirent.FileHandle, newDirectoryFileHandle, dirent.Name);
            }
        }

        return newDirectoryFileHandle;
    }

    public static async ValueTask<FileHandle> Copy(
        this IFileService fs,
        FileHandle fileHandle,
        FileHandle parentFileHandle,
        string name)
    {
        var stats = await fs.Stat(fileHandle);
        if (stats.Type.HasFlag(FileType.Directory))
        {
            return await fs.CopyDirectory(fileHandle, parentFileHandle, name);
        }

        return await fs.CopyFile(fileHandle, parentFileHandle, name);
    }
}
