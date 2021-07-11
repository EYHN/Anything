using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Provider;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class VirtualFileSystemServiceTests
    {
        [Test]
        public async Task CopyTest()
        {
            // init
            var vfs = new VirtualSystem(new MemoryFileSystemProvider(), TestUtils.CreateSqliteContext());
            await vfs.CreateDirectory(Url.Parse("memory://test/foo"));
            await vfs.CreateDirectory(Url.Parse("memory://test/foo/bar"));
            await vfs.CreateDirectory(Url.Parse("memory://test/foo/bar/sub"));
            await vfs.WriteFile(Url.Parse("memory://test/foo/bar/a"), Convert.FromHexString("010203"));
            await vfs.CreateDirectory(Url.Parse("memory://test/foo/bar2"));
            await vfs.WriteFile(Url.Parse("memory://test/foo/bar2/c"), Convert.FromHexString("030201"));

            // test
            await vfs.Copy(Url.Parse("memory://test/foo/bar/a"), Url.Parse("memory://test/foo/bar/sub/b"), false);
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar/a")),
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar/sub/b")),
                "copy out and source files should be the same content.");

            await vfs.Copy(Url.Parse("memory://test/foo/bar"), Url.Parse("memory://test/foo/bar3"), false);
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar/a")),
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar3/a")),
                "Should copy all files in it when copy a directory.");
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar/sub/b")),
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar3/sub/b")),
                "Should copy all files in it when copy a directory.");

            await vfs.Copy(Url.Parse("memory://test/foo/bar2/c"), Url.Parse("memory://test/foo/bar/a"), true);
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar2/c")),
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar/a")),
                "Should overwrite file content when overwrite is true.");

            await vfs.Copy(Url.Parse("memory://test/foo/bar2"), Url.Parse("memory://test/foo/bar"), true);
            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await vfs.ReadFile(Url.Parse("memory://test/foo/bar/a")),
                "Should overwrite all files in it when overwrite a directory.");
            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await vfs.ReadFile(Url.Parse("memory://test/foo/bar/sub/b")),
                "Should overwrite all files in it when overwrite a directory.");
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar/c")),
                await vfs.ReadFile(Url.Parse("memory://test/foo/bar2/c")));

            Assert.ThrowsAsync<FileExistsException>(
                async () => await vfs.Copy(Url.Parse("memory://test/foo/bar2/c"), Url.Parse("memory://test/foo/bar/c"), false),
                "Should throws when destination exists and overwrite is false");
            Assert.ThrowsAsync<FileExistsException>(
                async () => await vfs.Copy(Url.Parse("memory://test/foo/bar2"), Url.Parse("memory://test/foo/bar"), false),
                "Should throws when destination exists and overwrite is false");
            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await vfs.Copy(Url.Parse("memory://test/foo/bar99"), Url.Parse("memory://test/foo/bar"), true),
                "Should throws when source not exists");
        }
    }
}
