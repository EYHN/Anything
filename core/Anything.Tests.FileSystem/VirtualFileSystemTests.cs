using System;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Impl;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class VirtualFileSystemTests
    {
        [Test]
        public async Task CopyTest()
        {
            // init
            using var trackerCacheContext = new SqliteContext();
            using var tracker = new DatabaseHintFileTracker(trackerCacheContext);
            using var vfs = new VirtualFileSystem(Url.Parse("file://test"), new MemoryFileSystemProvider(), tracker);
            await vfs.CreateDirectory(Url.Parse("file://test/foo"));
            await vfs.CreateDirectory(Url.Parse("file://test/foo/bar"));
            await vfs.CreateDirectory(Url.Parse("file://test/foo/bar/sub"));
            await vfs.WriteFile(Url.Parse("file://test/foo/bar/a"), Convert.FromHexString("010203"));
            await vfs.CreateDirectory(Url.Parse("file://test/foo/bar2"));
            await vfs.WriteFile(Url.Parse("file://test/foo/bar2/c"), Convert.FromHexString("030201"));

            // test
            await vfs.Copy(Url.Parse("file://test/foo/bar/a"), Url.Parse("file://test/foo/bar/sub/b"), false);
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("file://test/foo/bar/a")),
                await vfs.ReadFile(Url.Parse("file://test/foo/bar/sub/b")),
                "copy out and source files should be the same content.");

            await vfs.Copy(Url.Parse("file://test/foo/bar"), Url.Parse("file://test/foo/bar3"), false);
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("file://test/foo/bar/a")),
                await vfs.ReadFile(Url.Parse("file://test/foo/bar3/a")),
                "Should copy all files in it when copy a directory.");
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("file://test/foo/bar/sub/b")),
                await vfs.ReadFile(Url.Parse("file://test/foo/bar3/sub/b")),
                "Should copy all files in it when copy a directory.");

            await vfs.Copy(Url.Parse("file://test/foo/bar2/c"), Url.Parse("file://test/foo/bar/a"), true);
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("file://test/foo/bar2/c")),
                await vfs.ReadFile(Url.Parse("file://test/foo/bar/a")),
                "Should overwrite file content when overwrite is true.");

            await vfs.Copy(Url.Parse("file://test/foo/bar2"), Url.Parse("file://test/foo/bar"), true);
            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await vfs.ReadFile(Url.Parse("file://test/foo/bar/a")),
                "Should overwrite all files in it when overwrite a directory.");
            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await vfs.ReadFile(Url.Parse("file://test/foo/bar/sub/b")),
                "Should overwrite all files in it when overwrite a directory.");
            Assert.AreEqual(
                await vfs.ReadFile(Url.Parse("file://test/foo/bar/c")),
                await vfs.ReadFile(Url.Parse("file://test/foo/bar2/c")));

            Assert.ThrowsAsync<FileExistsException>(
                async () => await vfs.Copy(Url.Parse("file://test/foo/bar2/c"), Url.Parse("file://test/foo/bar/c"), false),
                "Should throws when destination exists and overwrite is false");
            Assert.ThrowsAsync<FileExistsException>(
                async () => await vfs.Copy(Url.Parse("file://test/foo/bar2"), Url.Parse("file://test/foo/bar"), false),
                "Should throws when destination exists and overwrite is false");
            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await vfs.Copy(Url.Parse("file://test/foo/bar99"), Url.Parse("file://test/foo/bar"), true),
                "Should throws when source not exists");
        }

        [Test]
        public async Task FileSystemScanTest()
        {
            var rawfs = new MemoryFileSystemProvider();

            // create file before vfs create.
            await rawfs.CreateDirectory(Url.Parse("file://test/foo"));
            await rawfs.CreateDirectory(Url.Parse("file://test/foo/bar"));
            await rawfs.WriteFile(Url.Parse("file://test/foo/a"), Convert.FromHexString("010203"));
            await rawfs.WriteFile(Url.Parse("file://test/foo/bar/b"), Convert.FromHexString("010203"));

            using var trackerCacheContext = new SqliteContext();
            using var tracker = new DatabaseHintFileTracker(trackerCacheContext);
            using var vfs = new VirtualFileSystem(Url.Parse("file://test"), rawfs, tracker);

            var fileEventsHandler = new FileEventsHandler();
            vfs.FileEvent.On(fileEventsHandler.HandleFileEvents);

            await vfs.WaitFullScan();
            await vfs.WaitComplete();

            fileEventsHandler.AssertWithEvent(new[]
            {
                new FileEvent(FileEvent.EventType.Created, Url.Parse("file://test/foo")),
                new FileEvent(FileEvent.EventType.Created, Url.Parse("file://test/foo/bar")),
                new FileEvent(FileEvent.EventType.Created, Url.Parse("file://test/foo/a")),
                new FileEvent(FileEvent.EventType.Created, Url.Parse("file://test/foo/bar/b"))
            });
        }
    }
}
