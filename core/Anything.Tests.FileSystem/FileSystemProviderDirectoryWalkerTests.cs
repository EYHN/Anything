using System;
using System.Threading.Tasks;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Walker;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class FileSystemProviderDirectoryWalkerTests
    {
        private async ValueTask<IFileSystemProvider> TestFileSystem()
        {
            var fileSystem = new MemoryFileSystemProvider();
            await fileSystem.CreateDirectory(Url.Parse("file://test/foo"));
            await fileSystem.CreateDirectory(Url.Parse("file://test/foo/bar"));
            await fileSystem.CreateDirectory(Url.Parse("file://test/foo/bar/dir"));
            await fileSystem.WriteFile(Url.Parse("file://test/foo/bar/a"), Convert.FromHexString("010203"));
            await fileSystem.WriteFile(Url.Parse("file://test/foo/bar/b"), Convert.FromHexString("010203"));
            await fileSystem.WriteFile(Url.Parse("file://test/foo/bar/c"), Convert.FromHexString("010203"));
            await fileSystem.WriteFile(Url.Parse("file://test/foo/bar/dir/a"), Convert.FromHexString("010203"));
            await fileSystem.WriteFile(Url.Parse("file://test/foo/bar/dir/b"), Convert.FromHexString("010203"));
            return fileSystem;
        }

        [Test]
        public async Task FeatureTest()
        {
            var walker = new FileSystemProviderDirectoryWalker(await TestFileSystem(), Url.Parse("file://test/"));

            await foreach (var item in walker)
            {
                Console.WriteLine(item.Url);
                foreach (var (name, stats) in item.Entries)
                {
                    Console.WriteLine($"\t{name}\t\t{stats.Type}");
                }
            }
        }

        [Test]
        public async Task CallbackThrowsTest()
        {
            var callbackCount = 0;
            var walker = new FileSystemProviderDirectoryWalker(await TestFileSystem(), Url.Parse("file://test/"));
            using var walkerThread = walker.StartWalkerThread((item) =>
                {
                    callbackCount++;

                    if (callbackCount % 2 == 0)
                    {
                        // throw exception should not affect the walker
                        throw new ApplicationException();
                    }

                    return Task.CompletedTask;
                });

            await walkerThread.WaitFullWalk();
        }
    }
}
