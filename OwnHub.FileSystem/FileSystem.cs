using System;
using System.Threading.Tasks;

namespace OwnHub.FileSystem
{
    public class FileSystem
    {
        public IFileSystemProvider FileSystemProvider { get; }

        public FileSystem(IFileSystemProvider fileSystemProvider)
        {
            FileSystemProvider = fileSystemProvider;
        }

        // public async ValueTask Copy(Uri source, Uri destination, bool overwrite)
        // {
        //     var sourceType = await FileSystemProvider.Stat(source);
        //     var destinationType = await FileSystemProvider.Stat(destination);
        //
        //     if (TryGetFile(sourcePathParts, out var sourceEntity))
        //     {
        //         if (TryGetFile(destinationPathParts.SkipLast(1), out var destinationParent) &&
        //             destinationParent is Directory destinationParentDirectory)
        //         {
        //             if (overwrite)
        //             {
        //                 destinationParentDirectory[destinationPathParts[^1]] = (Entity)sourceEntity.Clone();
        //             }
        //             else
        //             {
        //                 if (destinationParentDirectory.TryAdd(destinationPathParts[^1], (Entity)sourceEntity.Clone()) == false)
        //                 {
        //                     throw new FileExistsException(destination);
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             throw new FileNotFoundException('/' + string.Join('/', destinationPathParts.SkipLast(1)));
        //         }
        //     }
        //     else
        //     {
        //         throw new FileNotFoundException(source);
        //     }
        //
        //     return ValueTask.CompletedTask;
        // }
    }
}
