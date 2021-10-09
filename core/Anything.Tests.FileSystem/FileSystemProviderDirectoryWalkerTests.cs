using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.FileSystem.Walker;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class FileSystemProviderDirectoryWalkerTests
    {
        private static async ValueTask<IFileSystem> TestFileSystem()
        {
            var fileSystem = new MemoryFileSystem();
            var root = await fileSystem.CreateFileHandle("/");
            var foo = await fileSystem.CreateDirectory(root, "foo");
            var bar = await fileSystem.CreateDirectory(foo, "bar");
            var dir = await fileSystem.CreateDirectory(bar, "dir");
            await fileSystem.CreateFile(bar, "a", Convert.FromHexString("010203"));
            await fileSystem.CreateFile(bar, "b", Convert.FromHexString("010203"));
            await fileSystem.CreateFile(bar, "c", Convert.FromHexString("010203"));
            await fileSystem.CreateFile(dir, "a", Convert.FromHexString("010203"));
            await fileSystem.CreateFile(dir, "b", Convert.FromHexString("010203"));
            return fileSystem;
        }

        [Test]
        public async Task FeatureTest()
        {
            var fs = await TestFileSystem();
            var walker = new FileSystemDirectoryWalker(fs, await fs.CreateFileHandle("/"));

            await foreach (var item in walker)
            {
                Console.WriteLine(item.FileHandle);
                foreach (var dirent in item.Entries)
                {
                    Console.WriteLine($"\t{dirent.Name}\t\t{dirent.Stats.Type}");
                }
            }
        }

        [Test]
        public async Task CallbackThrowsTest()
        {
            var callbackCount = 0;
            var fs = await TestFileSystem();
            var walker = new FileSystemDirectoryWalker(fs, await fs.CreateFileHandle("/"));
            using var walkerThread = walker.StartWalkerThread(_ =>
            {
                callbackCount++;

                if (callbackCount % 2 == 0)
                {
                    // throw exception should not affect the walker
                    throw new InvalidOperationException();
                }

                return Task.CompletedTask;
            });

            await walkerThread.WaitFullWalk();
        }
    }
}
