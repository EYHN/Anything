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
        [Test]
        public async Task FeatureTest()
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

            var walker = new FileSystemProviderDirectoryWalker(fileSystem, Url.Parse("file://test/"));

            await foreach (var item in walker)
            {
                Console.WriteLine(item.Url);
                foreach (var (name, stats) in item.Entries)
                {
                    Console.WriteLine($"\t{name}\t\t{stats.Type}");
                }
            }
        }
    }
}
