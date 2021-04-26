using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.FileSystem;
using OwnHub.FileSystem.Exception;
using OwnHub.FileSystem.Local;
using OwnHub.FileSystem.Memory;

namespace OwnHub.Tests.FileSystem
{
    public class FileSystemProviderTests
    {
        [Test]
        public async Task FeatureTest()
        {
            await RunCorrectnessTest(new MemoryFileSystemProvider());
        }

        [Test]
        public async Task LocalFileSystemProviderTest()
        {
            await RunCorrectnessTest(new LocalFileSystemProvider(TestUtils.GetTestDirectoryPath()));
        }

        /// <summary>
        /// Testing the file system provider is correctly implemented.
        /// </summary>
        private static async Task RunCorrectnessTest(IFileSystemProvider provider, bool strictExceptionType = false)
        {
            // Create directory test.
            {
                await provider.CreateDirectory(new Uri("file://test/foo/bar"));
                Assert.DoesNotThrowAsync(async () => await provider.CreateDirectory(new Uri("file://test/foo/bar")));
            }

            // Create file test.
            {
                await provider.WriteFile(new Uri("file://test/foo/bar/a"), Convert.FromHexString("010203"));
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.WriteFile(new Uri("file://test/foo/bar/a/c"), Convert.FromHexString("010203")));
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.WriteFile(new Uri("file://test/foo/a/c"), Convert.FromHexString("010203")));
            }

            // Read file test.
            {
                var content = await provider.ReadFile(new Uri("file://test/foo/bar/a"));
                Assert.AreEqual(Convert.FromHexString("010203"), content);

                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileIsADirectoryException) : typeof(FileSystemException),
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar")));
            }

            // Delete file test.
            {
                await provider.WriteFile(new Uri("file://test/foo/bar/b"), Convert.FromHexString("010203"));
                Assert.DoesNotThrowAsync(async () => await provider.ReadFile(new Uri("file://test/foo/bar/b")));
                await provider.Delete(new Uri("file://test/foo/bar/b"), false);
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar/b")));

                await provider.CreateDirectory(new Uri("file://test/foo/bar/b/c"));
                await provider.WriteFile(new Uri("file://test/foo/bar/b/d"), Convert.FromHexString("010203"));
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileIsADirectoryException) : typeof(FileSystemException),
                    async () => await provider.Delete(new Uri("file://test/foo/bar/b"), false),
                    "Should throws FileIsADirectoryException when delete a directory that has files and the recursive is false.");
                Assert.DoesNotThrowAsync(
                    async () => await provider.Delete(new Uri("file://test/foo/bar/c"), false),
                    "Should not throws exception when delete a empty directory and the recursive is false.");

                Assert.DoesNotThrowAsync(
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar/b/d")));
                await provider.Delete(new Uri("file://test/foo/bar/b"), true);
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar/b/d")));

                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.Delete(new Uri("file://test/foo/bar/b/d"), true));
            }

            // Copy test.
            {
                await provider.Copy(new Uri("file://test/foo/bar/a"), new Uri("file://test/foo/bar/b"), false);
                Assert.AreEqual(
                    await provider.ReadFile(new Uri("file://test/foo/bar/a")),
                    await provider.ReadFile(new Uri("file://test/foo/bar/b")));

                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileExistsException) : typeof(FileSystemException),
                    async () => await provider.Copy(new Uri("file://test/foo/bar/b"), new Uri("file://test/foo/bar/a"), false));

                await provider.Copy(new Uri("file://test/foo/bar"), new Uri("file://test/foo/bar2"), false);

                // clean up
                await provider.Delete(new Uri("file://test/foo/bar/b"), false);
                await provider.Delete(new Uri("file://test/foo/bar/bar2"), true);
            }

            // Rename test.
            {
                await provider.Rename(new Uri("file://test/foo/bar/a"), new Uri("file://test/foo/bar/b"), false);
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar/a")));
                Assert.AreEqual(Convert.FromHexString("010203"), await provider.ReadFile(new Uri("file://test/foo/bar/b")));

                // Overwrite test.
                await provider.WriteFile(new Uri("file://test/foo/bar/c"), Convert.FromHexString("030201"));
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileExistsException) : typeof(FileSystemException),
                    async () => await provider.Rename(new Uri("file://test/foo/bar/b"), new Uri("file://test/foo/bar/c"), false));
                await provider.Rename(new Uri("file://test/foo/bar/b"), new Uri("file://test/foo/bar/c"), true);
                Assert.AreEqual(Convert.FromHexString("010203"), await provider.ReadFile(new Uri("file://test/foo/bar/c")));
                await provider.Rename(new Uri("file://test/foo/bar/c"), new Uri("file://test/foo/bar/a"), false);
            }

            // File stats and modify file test.
            {
                var oldStats = await provider.Stat(new Uri("file://test/foo/bar/a"));
                await Task.Delay(100);
                await provider.WriteFile(new Uri("file://test/foo/bar/a"), Convert.FromHexString("01020304"));
                var newStats = await provider.Stat(new Uri("file://test/foo/bar/a"));
                Assert.Less(oldStats.LastWriteTime, newStats.LastWriteTime);
                Assert.AreEqual(oldStats.CreationTime, newStats.CreationTime);
                Assert.AreEqual(3, oldStats.Size);
                Assert.AreEqual(4, newStats.Size);
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.WriteFile(new Uri("file://test/foo/bar/a/c"), Convert.FromHexString("010203")));
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.WriteFile(new Uri("file://test/foo/a/c"), Convert.FromHexString("010203")));
            }

            // Directory stats test.
            {
                var oldParentDirectoryStats = await provider.Stat(new Uri("file://test/foo"));
                var oldDirectoryStats = await provider.Stat(new Uri("file://test/foo/bar"));

                await Task.Delay(100);
                await provider.WriteFile(new Uri("file://test/foo/bar/a"), Convert.FromHexString("010203"));
                var newDirectoryStats = await provider.Stat(new Uri("file://test/foo/bar"));
                Assert.AreEqual(oldDirectoryStats.LastWriteTime, newDirectoryStats.LastWriteTime);
                Assert.AreEqual(oldDirectoryStats.CreationTime, newDirectoryStats.CreationTime);

                await provider.CreateDirectory(new Uri("file://test/foo/bar/b"));
                newDirectoryStats = await provider.Stat(new Uri("file://test/foo/bar"));
                Assert.Less(oldDirectoryStats.LastWriteTime, newDirectoryStats.LastWriteTime);
                Assert.AreEqual(oldDirectoryStats.CreationTime, newDirectoryStats.CreationTime);
                oldDirectoryStats = newDirectoryStats;

                await Task.Delay(100);
                await provider.Delete(new Uri("file://test/foo/bar/b"), false);
                newDirectoryStats = await provider.Stat(new Uri("file://test/foo/bar"));
                Assert.Less(oldDirectoryStats.LastWriteTime, newDirectoryStats.LastWriteTime);
                Assert.AreEqual(oldDirectoryStats.CreationTime, newDirectoryStats.CreationTime);

                var newParentDirectoryStats = await provider.Stat(new Uri("file://test/foo"));
                Assert.AreEqual(oldParentDirectoryStats.CreationTime, newParentDirectoryStats.CreationTime);
                Assert.AreEqual(oldParentDirectoryStats.CreationTime, newParentDirectoryStats.CreationTime);
            }

            // Read directory test.
            {
                await provider.CreateDirectory(new Uri("file://test/foo/bar/b"));

                var dir = new Dictionary<string, FileType>(await provider.ReadDirectory(new Uri("file://test/foo/bar")));

                Assert.AreEqual(2, dir.Count);

                Assert.AreEqual(FileType.File, dir["a"]);
                Assert.AreEqual(FileType.Directory, dir["b"]);

                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotFoundException) : typeof(FileSystemException),
                    async () => await provider.ReadDirectory(new Uri("file://test/foo/bar/c")));
                Assert.ThrowsAsync(
                    strictExceptionType ? typeof(FileNotADirectoryException) : typeof(FileSystemException),
                    async () => await provider.ReadDirectory(new Uri("file://test/foo/bar/a")));

                // Clean up
                await provider.Delete(new Uri("file://test/foo/bar/b"), false);
            }
        }
    }
}
