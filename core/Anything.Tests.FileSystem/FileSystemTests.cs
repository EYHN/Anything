using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Impl;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;
using NUnit.Framework;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.Tests.FileSystem
{
    [TestFixtureSource(nameof(FileSystemCases))]
    public class FileSystemTests
    {
        private readonly IFileSystem _fileSystem;

        public static IEnumerable FileSystemCases
        {
            get
            {
                yield return new MemoryFileSystem();
                using var trackerStorage =
                    new HintFileTracker.LocalStorage(Path.Join(TestUtils.GetTestDirectoryPath("LocalFileSystemTests"), "tracker.db"));
                yield return new LocalFileSystem(TestUtils.GetTestDirectoryPath("LocalFileSystemTests"), trackerStorage);
            }
        }

        public FileSystemTests(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        [Test]
        public async Task CreateDirectoryTests()
        {
            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "create_directory");
            await _fileSystem.CreateDirectory(root, "test_dir");
            Assert.ThrowsAsync<FileExistsException>(async () => await _fileSystem.CreateDirectory(root, "test_dir"));
        }

        [Test]
        public async Task CreateWriteFileTests()
        {
            var fileContent = Convert.FromHexString("010203");
            var fileContent2 = Convert.FromHexString("040506");

            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "create_write_file");

            var testFile = await _fileSystem.CreateFile(root, "test_file", fileContent);
            await _fileSystem.WriteFile(testFile, fileContent);
            await _fileSystem.WriteFile(testFile, fileContent2);
            Assert.ThrowsAsync<FileExistsException>(
                async () => await _fileSystem.CreateFile(
                    root,
                    "test_file",
                    fileContent));
            await _fileSystem.CreateDirectory(root, "test_dir");
            Assert.ThrowsAsync<FileExistsException>(
                async () => await _fileSystem.CreateFile(
                    root,
                    "test_dir",
                    fileContent));
            Assert.ThrowsAsync<FileIsADirectoryException>(
                async () => await _fileSystem.WriteFile(
                    root,
                    fileContent));
            Assert.ThrowsAsync<FileNotADirectoryException>(
                async () => await _fileSystem.CreateFile(
                    testFile,
                    "test_file",
                    fileContent));
        }

        [Test]
        public async Task ReadFileTests()
        {
            var fileContent = Convert.FromHexString("010203");

            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "read_file");

            var testFile = await _fileSystem.CreateFile(root, "test_file", fileContent);
            var content = await _fileSystem.ReadFile(testFile);
            Assert.AreEqual(fileContent, content.ToArray());
        }

        [Test]
        public async Task DeleteFileTests()
        {
            var fileContent = Convert.FromHexString("010203");

            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "delete_file");

            var testFile = await _fileSystem.CreateFile(root, "test", fileContent);
            Assert.DoesNotThrowAsync(async () => await _fileSystem.ReadFile(testFile));
            await _fileSystem.Delete(testFile, root, "test", false);
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileSystem.Delete(testFile, root, "test", false));
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileSystem.ReadFile(testFile));

            var testDirectory = await _fileSystem.CreateDirectory(root, "test");
            Assert.DoesNotThrowAsync(async () => await _fileSystem.ReadDirectory(testDirectory));
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileSystem.Delete(testFile, root, "test", false));

            var testFile2 = await _fileSystem.CreateFile(testDirectory, "test_file", fileContent);
            var testDirectory2 = await _fileSystem.CreateDirectory(testDirectory, "test_directory");

            Assert.DoesNotThrowAsync(async () => await _fileSystem.ReadFile(testFile2));
            Assert.DoesNotThrowAsync(async () => await _fileSystem.ReadDirectory(testDirectory2));
            Assert.ThrowsAsync<FileIsADirectoryException>(async () => await _fileSystem.Delete(testDirectory, root, "test", false));
            await _fileSystem.Delete(testDirectory, root, "test", true);

            Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileSystem.Delete(testDirectory, root, "test", false));

            Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileSystem.ReadFile(testFile2));
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileSystem.ReadFile(testDirectory2));
        }

        [Test]
        public async Task RenameTests()
        {
            var fileContent = Convert.FromHexString("010203");
            var fileContent2 = Convert.FromHexString("040506");

            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "rename");

            var fooDir = await _fileSystem.CreateDirectory(root, "foo");

            var aFile = await _fileSystem.CreateFile(fooDir, "a", fileContent);
            var bFile = await _fileSystem.Rename(aFile, fooDir, "a", fooDir, "b");
            Assert.AreEqual(fileContent, (await _fileSystem.ReadFile(bFile)).ToArray());
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await _fileSystem.ReadFile(await _fileSystem.CreateFileHandle("/rename/foo/a")));
            aFile = await _fileSystem.CreateFile(
                fooDir,
                "a",
                fileContent2);
            Assert.DoesNotThrowAsync(async () =>
                await _fileSystem.ReadFile(await _fileSystem.CreateFileHandle("/rename/foo/a")));
            Assert.ThrowsAsync<FileExistsException>(
                async () => await _fileSystem.Rename(aFile, fooDir, "a", fooDir, "b"));

            await _fileSystem.Rename(fooDir, root, "foo", root, "foo2");
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await _fileSystem.ReadFile(await _fileSystem.CreateFileHandle("/rename/foo/a")));
            Assert.DoesNotThrowAsync(async () =>
                await _fileSystem.ReadFile(await _fileSystem.CreateFileHandle("/rename/foo2/a")));
            Assert.DoesNotThrowAsync(async () =>
                await _fileSystem.ReadFile(await _fileSystem.CreateFileHandle("/rename/foo2/b")));
        }

        [Test]
        public async Task StatTests()
        {
            var fileContent = Convert.FromHexString("010203");

            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "stat");

            var testFile = await _fileSystem.CreateFile(root, "test_file", fileContent);
            var testDir = await _fileSystem.CreateDirectory(root, "test_dir");
            var fileStats = await _fileSystem.Stat(testFile);
            var dirStats = await _fileSystem.Stat(testDir);

            Assert.AreEqual(dirStats.Type, FileType.Directory);
            Assert.AreEqual(fileStats.Type, FileType.File);
        }

        [Test]
        public async Task ReadDirectoryTests()
        {
            var fileContent = Convert.FromHexString("010203");
            var fileContent2 = Convert.FromHexString("040506");

            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "read_directory");

            var testDir = await _fileSystem.CreateDirectory(root, "test_dir");
            var a = await _fileSystem.CreateFile(testDir, "a", fileContent);
            await _fileSystem.CreateFile(testDir, "b", fileContent2);
            await _fileSystem.CreateDirectory(testDir, "c");

            var dirents = (await _fileSystem.ReadDirectory(testDir)).ToDictionary(d => d.Name, d => d);
            Assert.AreEqual(3, dirents.Count);
            Assert.AreEqual(FileType.File, dirents["a"].Stats.Type);
            Assert.AreEqual(FileType.File, dirents["b"].Stats.Type);
            Assert.AreEqual(FileType.Directory, dirents["c"].Stats.Type);

            Assert.ThrowsAsync<FileNotADirectoryException>(
                async () => await _fileSystem.ReadDirectory(a));
        }

        [Test]
        public async Task ReadFileStreamTests()
        {
            var fileContent = Convert.FromHexString("01020304");

            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "read_file_stream");
            var testFile = await _fileSystem.CreateFile(root, "test_file", fileContent);

            var returnMessage = await _fileSystem.ReadFileStream(testFile, stream =>
            {
                Assert.AreEqual(4, stream.Length);

                var data = new byte[4];
                stream.Read(data);
                Assert.AreEqual(fileContent, data);
                Assert.AreEqual(0, stream.Read(data));

                return ValueTask.FromResult("return_message");
            });

            Assert.AreEqual("return_message", returnMessage);

            Assert.ThrowsAsync<FileIsADirectoryException>(
                async () => await _fileSystem.ReadFileStream(
                    root,
                    _ => ValueTask.FromResult(true)));

            Assert.ThrowsAsync<AggregateException>(
                async () => await _fileSystem.ReadFileStream<object>(
                    testFile,
                    _ => throw new InvalidOperationException("reader exception")));
        }

        [Test]
        public async Task GetFilePathTests()
        {
            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "get_file_path");
            var testFile = await _fileSystem.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

            Assert.AreEqual("/get_file_path/test_file", await _fileSystem.GetFilePath(testFile));
            Assert.AreEqual("/", await _fileSystem.GetFilePath(await _fileSystem.CreateFileHandle("/")));
        }

        [Test]
        public async Task GetFileNameTests()
        {
            var root =
                await _fileSystem.CreateDirectory(await _fileSystem.CreateFileHandle("/"), "get_file_name");
            var testFile = await _fileSystem.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

            Assert.AreEqual("test_file", await _fileSystem.GetFileName(testFile));
        }
    }
}
