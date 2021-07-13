using System;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Provider;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class FileSystemProviderTests
    {
        [Test]
        public async Task MemoryFileSystemProviderTest()
        {
            await RunCorrectnessTest(new MemoryFileSystemProvider());
        }

        [Test]
        public async Task LocalFileSystemProviderTest()
        {
            await RunCorrectnessTest(new LocalFileSystemProvider(TestUtils.GetTestDirectoryPath()));
        }

        [Test]
        public async Task WrappedVirtualFileSystemServiceProviderTest()
        {
            var service = new VirtualFileSystem(new MemoryFileSystemProvider(), TestUtils.CreateSqliteContext());
            await RunCorrectnessTest(service);
        }

        /// <summary>
        ///     Testing the file system provider is correctly implemented.
        /// </summary>
        private static async Task RunCorrectnessTest(IFileSystemProvider provider)
        {
            // Create directory test.
            {
                await provider.CreateDirectory(Url.Parse("file://test/foo"));
                await provider.CreateDirectory(Url.Parse("file://test/foo/bar"));
                Assert.ThrowsAsync<FileExistsException>(async () => await provider.CreateDirectory(Url.Parse("file://test/foo/bar")));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.CreateDirectory(Url.Parse("file://test/foo/bar/a/b")));
            }

            // Write file test.
            {
                await provider.WriteFile(Url.Parse("file://test/foo/bar/a"), Convert.FromHexString("010203"));
                await provider.WriteFile(Url.Parse("file://test/foo/bar/a"), Convert.FromHexString("01020304"));
                Assert.ThrowsAsync<FileIsADirectoryException>(
                    async () => await provider.WriteFile(
                        Url.Parse("file://test/foo/bar"),
                        Convert.FromHexString("01020304"),
                        overwrite: true));
                Assert.ThrowsAsync<FileNotFoundException>(
                    async () => await provider.WriteFile(
                        Url.Parse("file://test/foo/bar/b"),
                        Convert.FromHexString("01020304"),
                        false));
                Assert.ThrowsAsync<FileExistsException>(
                    async () => await provider.WriteFile(
                        Url.Parse("file://test/foo/bar/a"),
                        Convert.FromHexString("01020304"),
                        overwrite: false));
                Assert.ThrowsAsync<FileNotFoundException>(
                    async () => await provider.WriteFile(Url.Parse("file://test/foo/bar/a/a/a"), Convert.FromHexString("01020304")));
            }

            // Read file test.
            {
                var content = await provider.ReadFile(Url.Parse("file://test/foo/bar/a"));
                Assert.AreEqual(Convert.FromHexString("01020304"), content);

                Assert.ThrowsAsync<FileIsADirectoryException>(
                    async () => await provider.ReadFile(Url.Parse("file://test/foo/bar")));
                Assert.ThrowsAsync<FileNotFoundException>(
                    async () => await provider.ReadFile(Url.Parse("file://test/foo/bar/c")));
            }

            // Delete file test.
            {
                await provider.WriteFile(Url.Parse("file://test/foo/bar/b"), Convert.FromHexString("01020304"));
                Assert.DoesNotThrowAsync(async () => await provider.ReadFile(Url.Parse("file://test/foo/bar/b")));
                await provider.Delete(Url.Parse("file://test/foo/bar/b"), false);
                Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.ReadFile(Url.Parse("file://test/foo/bar/b")));

                await provider.CreateDirectory(Url.Parse("file://test/foo/bar/b"));
                Assert.ThrowsAsync<FileIsADirectoryException>(async () => await provider.Delete(Url.Parse("file://test/foo/bar/b"), false));
                await provider.Delete(Url.Parse("file://test/foo/bar/b"), true);

                Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.Delete(Url.Parse("file://test/foo/bar/b"), false));
            }

            // Rename test
            {
                await provider.CreateDirectory(Url.Parse("file://test/foo/bar2"));
                await provider.WriteFile(Url.Parse("file://test/foo/bar2/a"), Convert.FromHexString("01020304"));

                var oldContent = await provider.ReadFile(Url.Parse("file://test/foo/bar/a"));
                await provider.Rename(Url.Parse("file://test/foo/bar/a"), Url.Parse("file://test/foo/bar/b"), false);
                Assert.AreEqual(oldContent, await provider.ReadFile(Url.Parse("file://test/foo/bar/b")));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.ReadFile(Url.Parse("file://test/foo/bar/a")));

                await provider.WriteFile(Url.Parse("file://test/foo/bar/a"), Convert.FromHexString("0102030405"));
                Assert.ThrowsAsync<FileExistsException>(
                    async () => await provider.Rename(Url.Parse("file://test/foo/bar/a"), Url.Parse("file://test/foo/bar/b"), false),
                    "Should throws when newUri exists and overwrite is false");
                await provider.Rename(Url.Parse("file://test/foo/bar/a"), Url.Parse("file://test/foo/bar/b"), true);
                Assert.AreEqual(Convert.FromHexString("0102030405"), await provider.ReadFile(Url.Parse("file://test/foo/bar/b")));

                Assert.ThrowsAsync<FileExistsException>(
                    async () => await provider.Rename(Url.Parse("file://test/foo/bar2"), Url.Parse("file://test/foo/bar"), false),
                    "Should throws when newUri exists and overwrite is false");

                await provider.Rename(Url.Parse("file://test/foo/bar2"), Url.Parse("file://test/foo/bar"), true);
                Assert.ThrowsAsync<FileNotFoundException>(
                    async () => await provider.ReadFile(Url.Parse("file://test/foo/bar/b")));

                Assert.ThrowsAsync<FileNotFoundException>(
                    async () => await provider.Rename(Url.Parse("file://test/foo/bar2"), Url.Parse("file://test/foo/bar"), true),
                    "Should throws when oldUri doesn't exists.");
                Assert.ThrowsAsync<FileNotFoundException>(
                    async () => await provider.Rename(Url.Parse("file://test/foo/bar"), Url.Parse("file://test/foo/bar/a/a"), true),
                    "Should throws when newUri doesn't exists.");
            }

            // Stat test
            {
                var directoryStats = await provider.Stat(Url.Parse("file://test/foo/bar"));
                var fileStats = await provider.Stat(Url.Parse("file://test/foo/bar/a"));
                Assert.AreEqual(directoryStats.Type, FileType.Directory);
                Assert.AreEqual(fileStats.Type, FileType.File);

                Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.Stat(Url.Parse("file://test/foo/bar/b")));
            }

            // Read directory test
            {
                await provider.CreateDirectory(Url.Parse("file://test/foo/bar/b"));
                var dir = (await provider.ReadDirectory(Url.Parse("file://test/foo/bar"))).ToDictionary(
                    p => p.Name,
                    p => p.Stats);
                Assert.AreEqual(2, dir.Count);
                Assert.AreEqual(FileType.File, dir["a"].Type);
                Assert.AreEqual(FileType.Directory, dir["b"].Type);

                Assert.ThrowsAsync<FileNotADirectoryException>(
                    async () => await provider.ReadDirectory(Url.Parse("file://test/foo/bar/a")));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.ReadDirectory(Url.Parse("file://test/foo/bar2")));
            }

            // stream test
            if (provider is IFileSystemProviderSupportStream providerSupportStream)
            {
                var stream = await providerSupportStream.OpenReadFileStream(Url.Parse("file://test/foo/bar/a"));
                Assert.AreEqual(4, stream.Length);

                var data = new byte[4];
                stream.Read(data);
                Assert.AreEqual(Convert.FromHexString("01020304"), data);
                Assert.AreEqual(0, stream.Read(data));
                stream.Close();

                Assert.ThrowsAsync<FileIsADirectoryException>(
                    async () => await providerSupportStream.OpenReadFileStream(Url.Parse("file://test/foo/bar")));
                Assert.ThrowsAsync<FileNotFoundException>(
                    async () => await providerSupportStream.OpenReadFileStream(Url.Parse("file://test/foo/bar/c")));
            }
        }
    }
}
