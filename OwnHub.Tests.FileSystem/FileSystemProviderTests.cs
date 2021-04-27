using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.FileSystem;
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
                await provider.CreateDirectory(new Uri("file://test/foo"));
                await provider.CreateDirectory(new Uri("file://test/foo/bar"));
                Assert.CatchAsync(async () => await provider.CreateDirectory(new Uri("file://test/foo/bar")));
                Assert.CatchAsync(async () => await provider.CreateDirectory(new Uri("file://test/foo/bar/a/b")));
            }

            // Write file test.
            {
                await provider.WriteFile(new Uri("file://test/foo/bar/a"), Convert.FromHexString("010203"));
                Assert.CatchAsync(
                    async () => await provider.WriteFile(
                        new Uri("file://test/foo/bar/b"), Convert.FromHexString("01020304"), create: false));
                Assert.CatchAsync(
                    async () => await provider.WriteFile(
                        new Uri("file://test/foo/bar/a"), Convert.FromHexString("01020304"), overwrite: false));
                Assert.CatchAsync(
                    async () => await provider.WriteFile(new Uri("file://test/foo/bar/c/a"), Convert.FromHexString("01020304")));
            }

            // Read file test.
            {
                var content = await provider.ReadFile(new Uri("file://test/foo/bar/a"));
                Assert.AreEqual(Convert.FromHexString("010203"), content);

                Assert.CatchAsync(
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar")));
                Assert.CatchAsync(
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar/c")));
            }

            // Delete file test.
            {
                await provider.WriteFile(new Uri("file://test/foo/bar/b"), Convert.FromHexString("01020304"));
                Assert.DoesNotThrowAsync(async () => await provider.ReadFile(new Uri("file://test/foo/bar/b")));
                await provider.Delete(new Uri("file://test/foo/bar/b"), false);
                Assert.CatchAsync(async () => await provider.ReadFile(new Uri("file://test/foo/bar/b")));

                await provider.CreateDirectory(new Uri("file://test/foo/bar/b"));
                Assert.CatchAsync(async () => await provider.Delete(new Uri("file://test/foo/bar/b"), false));
                await provider.Delete(new Uri("file://test/foo/bar/b"), true);

                Assert.CatchAsync(async () => await provider.Delete(new Uri("file://test/foo/bar/b"), false));
            }

            // Copy test
            {
                await provider.Copy(new Uri("file://test/foo/bar/a"), new Uri("file://test/foo/bar/b"), false);
                Assert.AreEqual(
                    await provider.ReadFile(new Uri("file://test/foo/bar/a")),
                    await provider.ReadFile(new Uri("file://test/foo/bar/b")));

                Assert.CatchAsync(
                    async () => await provider.Copy(
                        new Uri("file://test/foo/bar/b"),
                        new Uri("file://test/foo/bar/a"),
                        false));

                await provider.CreateDirectory(new Uri("file://test/foo/bar2"));
                await provider.WriteFile(new Uri("file://test/foo/bar2/c"), Convert.FromHexString("01020304"));

                Assert.CatchAsync(
                    async () => await provider.Copy(
                        new Uri("file://test/foo/bar"),
                        new Uri("file://test/foo/bar2"),
                        false));
                await provider.Copy(
                    new Uri("file://test/foo/bar"),
                    new Uri("file://test/foo/bar2"),
                    true);
                Assert.AreEqual(
                    await provider.ReadFile(new Uri("file://test/foo/bar/a")),
                    await provider.ReadFile(new Uri("file://test/foo/bar2/a")));
                Assert.AreEqual(
                    await provider.ReadFile(new Uri("file://test/foo/bar/a")),
                    await provider.ReadFile(new Uri("file://test/foo/bar2/b")));
                Assert.CatchAsync(async () => await provider.ReadFile(new Uri("file://test/foo/bar2/c")));

                Assert.CatchAsync(
                    async () => await provider.Copy(
                        new Uri("file://test/foo/bar3"),
                        new Uri("file://test/foo/bar4"),
                        false),
                    "Should throws when source doesn't exist.");
                Assert.CatchAsync(
                    async () => await provider.Copy(
                        new Uri("file://test/foo/bar"),
                        new Uri("file://test/foo/bar3/bar4"),
                        true),
                    "Should throws when parent of destination doesn't exist.");
            }

            // Rename test
            {
                var oldContent = await provider.ReadFile(new Uri("file://test/foo/bar/b"));
                await provider.Rename(new Uri("file://test/foo/bar/b"), new Uri("file://test/foo/bar/d"), false);
                Assert.AreEqual(oldContent, await provider.ReadFile(new Uri("file://test/foo/bar/d")));
                Assert.CatchAsync(async () => await provider.ReadFile(new Uri("file://test/foo/bar/b")));

                Assert.CatchAsync(
                    async () => await provider.Rename(
                        new Uri("file://test/foo/bar2"),
                        new Uri("file://test/foo/bar"),
                        false),
                    "Should throws when newUri doesn't exists and overwrite is false");

                await provider.Rename(new Uri("file://test/foo/bar2"), new Uri("file://test/foo/bar"), true);
                Assert.CatchAsync(
                    async () => await provider.ReadFile(new Uri("file://test/foo/bar/d")));

                Assert.CatchAsync(
                    async () => await provider.Rename(new Uri("file://test/foo/bar2"), new Uri("file://test/foo/bar"), true),
                    "Should throws when oldUri doesn't exists.");
            }

            // Stat test
            {
                var directoryStats = await provider.Stat(new Uri("file://test/foo/bar"));
                var fileStats = await provider.Stat(new Uri("file://test/foo/bar/a"));
                Assert.AreEqual(directoryStats.Type, FileType.Directory);
                Assert.AreEqual(fileStats.Type, FileType.File);
            }
        }
    }
}
