using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Property;
using Anything.FileSystem.Singleton;
using Anything.FileSystem.Singleton.Impl;
using Anything.FileSystem.Singleton.Tracker;
using Anything.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.Tests.FileSystem;

[TestFixtureSource(nameof(FileSystemCases))]
public class FileSystemTests
{
    private readonly ISingletonFileSystem _singletonFileSystem;

    public static IEnumerable FileSystemCases
    {
        get
        {
            yield return new MemoryFileSystem(new NullFileEventService());
            yield return new LocalFileSystem(
                TestUtils.GetTestDirectoryPath("LocalFileSystemTests"),
                new NullFileEventService(),
                new NullLogger<LocalFileSystem>());
        }
    }

    public FileSystemTests(ISingletonFileSystem singletonFileSystem)
    {
        _singletonFileSystem = singletonFileSystem;
    }

    [Test]
    public async Task CreateDirectoryTests()
    {
        var root =
            await _singletonFileSystem.CreateDirectory(
                await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
                "create_directory");
        await _singletonFileSystem.CreateDirectory(root, "test_dir");
        Assert.ThrowsAsync<FileExistsException>(async () => await _singletonFileSystem.CreateDirectory(root, "test_dir"));
    }

    [Test]
    public async Task CreateWriteFileTests()
    {
        var fileContent = Convert.FromHexString("010203");
        var fileContent2 = Convert.FromHexString("040506");

        var root =
            await _singletonFileSystem.CreateDirectory(
                await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
                "create_write_file");

        var testFile = await _singletonFileSystem.CreateFile(root, "test_file", fileContent);
        await _singletonFileSystem.WriteFile(testFile, fileContent);
        await _singletonFileSystem.WriteFile(testFile, fileContent2);
        Assert.ThrowsAsync<FileExistsException>(
            async () => await _singletonFileSystem.CreateFile(
                root,
                "test_file",
                fileContent));
        await _singletonFileSystem.CreateDirectory(root, "test_dir");
        Assert.ThrowsAsync<FileExistsException>(
            async () => await _singletonFileSystem.CreateFile(
                root,
                "test_dir",
                fileContent));
        Assert.ThrowsAsync<FileIsADirectoryException>(
            async () => await _singletonFileSystem.WriteFile(
                root,
                fileContent));
        Assert.ThrowsAsync<FileNotADirectoryException>(
            async () => await _singletonFileSystem.CreateFile(
                testFile,
                "test_file",
                fileContent));
    }

    [Test]
    public async Task ReadFileTests()
    {
        var fileContent = Convert.FromHexString("010203");

        var root =
            await _singletonFileSystem.CreateDirectory(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")), "read_file");

        var testFile = await _singletonFileSystem.CreateFile(root, "test_file", fileContent);
        var content = await _singletonFileSystem.ReadFile(testFile);
        Assert.AreEqual(fileContent, content.ToArray());
    }

    [Test]
    public async Task DeleteFileTests()
    {
        var fileContent = Convert.FromHexString("010203");

        var root =
            await _singletonFileSystem.CreateDirectory(
                await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
                "delete_file");

        var testFile = await _singletonFileSystem.CreateFile(root, "test", fileContent);
        Assert.DoesNotThrowAsync(async () => await _singletonFileSystem.ReadFile(testFile));
        await _singletonFileSystem.Delete(testFile, root, "test", false);
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _singletonFileSystem.Delete(testFile, root, "test", false));
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _singletonFileSystem.ReadFile(testFile));

        var testDirectory = await _singletonFileSystem.CreateDirectory(root, "test");
        Assert.DoesNotThrowAsync(async () => await _singletonFileSystem.ReadDirectory(testDirectory));
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _singletonFileSystem.Delete(testFile, root, "test", false));

        var testFile2 = await _singletonFileSystem.CreateFile(testDirectory, "test_file", fileContent);
        var testDirectory2 = await _singletonFileSystem.CreateDirectory(testDirectory, "test_directory");

        Assert.DoesNotThrowAsync(async () => await _singletonFileSystem.ReadFile(testFile2));
        Assert.DoesNotThrowAsync(async () => await _singletonFileSystem.ReadDirectory(testDirectory2));
        Assert.ThrowsAsync<FileIsADirectoryException>(async () => await _singletonFileSystem.Delete(testDirectory, root, "test", false));
        await _singletonFileSystem.Delete(testDirectory, root, "test", true);

        Assert.ThrowsAsync<FileNotFoundException>(async () => await _singletonFileSystem.Delete(testDirectory, root, "test", false));

        Assert.ThrowsAsync<FileNotFoundException>(async () => await _singletonFileSystem.ReadFile(testFile2));
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _singletonFileSystem.ReadFile(testDirectory2));
    }

    [Test]
    public async Task RenameTests()
    {
        var fileContent = Convert.FromHexString("010203");
        var fileContent2 = Convert.FromHexString("040506");

        var root =
            await _singletonFileSystem.CreateDirectory(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")), "rename");

        var fooDir = await _singletonFileSystem.CreateDirectory(root, "foo");

        var aFile = await _singletonFileSystem.CreateFile(fooDir, "a", fileContent);
        var bFile = await _singletonFileSystem.Rename(aFile, fooDir, "a", fooDir, "b");
        Assert.AreEqual(fileContent, (await _singletonFileSystem.ReadFile(bFile)).ToArray());
        Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await _singletonFileSystem.ReadFile(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/rename/foo/a"))));
        aFile = await _singletonFileSystem.CreateFile(
            fooDir,
            "a",
            fileContent2);
        Assert.DoesNotThrowAsync(async () =>
            await _singletonFileSystem.ReadFile(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/rename/foo/a"))));
        Assert.ThrowsAsync<FileExistsException>(
            async () => await _singletonFileSystem.Rename(aFile, fooDir, "a", fooDir, "b"));

        await _singletonFileSystem.Rename(fooDir, root, "foo", root, "foo2");
        Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await _singletonFileSystem.ReadFile(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/rename/foo/a"))));
        Assert.DoesNotThrowAsync(async () =>
            await _singletonFileSystem.ReadFile(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/rename/foo2/a"))));
        Assert.DoesNotThrowAsync(async () =>
            await _singletonFileSystem.ReadFile(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/rename/foo2/b"))));
    }

    [Test]
    public async Task StatTests()
    {
        var fileContent = Convert.FromHexString("010203");

        var root =
            await _singletonFileSystem.CreateDirectory(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")), "stat");

        var testFile = await _singletonFileSystem.CreateFile(root, "test_file", fileContent);
        var testDir = await _singletonFileSystem.CreateDirectory(root, "test_dir");
        var fileStats = await _singletonFileSystem.Stat(testFile);
        var dirStats = await _singletonFileSystem.Stat(testDir);

        Assert.AreEqual(dirStats.Type, FileType.Directory);
        Assert.AreEqual(fileStats.Type, FileType.File);
    }

    [Test]
    public async Task ReadDirectoryTests()
    {
        var fileContent = Convert.FromHexString("010203");
        var fileContent2 = Convert.FromHexString("040506");

        var root =
            await _singletonFileSystem.CreateDirectory(
                await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
                "read_directory");

        var testDir = await _singletonFileSystem.CreateDirectory(root, "test_dir");
        var a = await _singletonFileSystem.CreateFile(testDir, "a", fileContent);
        await _singletonFileSystem.CreateFile(testDir, "b", fileContent2);
        await _singletonFileSystem.CreateDirectory(testDir, "c");

        var dirents = (await _singletonFileSystem.ReadDirectory(testDir)).ToDictionary(d => d.Name, d => d);
        Assert.AreEqual(3, dirents.Count);
        Assert.AreEqual(FileType.File, dirents["a"].Stats.Type);
        Assert.AreEqual(FileType.File, dirents["b"].Stats.Type);
        Assert.AreEqual(FileType.Directory, dirents["c"].Stats.Type);

        Assert.ThrowsAsync<FileNotADirectoryException>(
            async () => await _singletonFileSystem.ReadDirectory(a));
    }

    [Test]
    public async Task ReadFileStreamTests()
    {
        var fileContent = Convert.FromHexString("01020304");

        var root =
            await _singletonFileSystem.CreateDirectory(
                await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
                "read_file_stream");
        var testFile = await _singletonFileSystem.CreateFile(root, "test_file", fileContent);

        var returnMessage = await _singletonFileSystem.ReadFileStream(testFile, stream =>
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
            async () => await _singletonFileSystem.ReadFileStream(
                root,
                _ => ValueTask.FromResult(true)));

        Assert.ThrowsAsync<AggregateException>(
            async () => await _singletonFileSystem.ReadFileStream<object>(
                testFile,
                _ => throw new InvalidOperationException("reader exception")));
    }

    [Test]
    public async Task GetFileUrlTests()
    {
        var root =
            await _singletonFileSystem.CreateDirectory(
                await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
                "get_file_path");
        var testFile = await _singletonFileSystem.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

        Assert.AreEqual("/get_file_path/test_file", (await _singletonFileSystem.GetUrl(testFile)).Path);
        Assert.AreEqual(
            "/",
            (await _singletonFileSystem.GetUrl(await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")))).Path);
    }

    [Test]
    public async Task GetFileNameTests()
    {
        var root =
            await _singletonFileSystem.CreateDirectory(
                await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
                "get_file_name");
        var testFile = await _singletonFileSystem.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

        Assert.AreEqual("test_file", await _singletonFileSystem.GetFileName(testFile));
    }

    [Test]
    public async Task PropertyTests()
    {
        var root = await _singletonFileSystem.CreateDirectory(
            await _singletonFileSystem.CreateFileHandle(Url.Parse("file://test/")),
            "property");

        var testFile = await _singletonFileSystem.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

        await _singletonFileSystem.SetProperty(testFile, "hello", Encoding.UTF8.GetBytes("world"));
        Assert.AreEqual((await _singletonFileSystem.GetProperty(testFile, "hello"))!.Value.ToArray(), Encoding.UTF8.GetBytes("world"));
        Assert.Null(await _singletonFileSystem.GetProperty(testFile, "hello1"));

        await _singletonFileSystem.RemoveProperty(testFile, "hello");
        Assert.Null(await _singletonFileSystem.GetProperty(testFile, "hello"));

        await _singletonFileSystem.SetProperty(
            testFile,
            "delete-on-update",
            Encoding.UTF8.GetBytes("123"),
            PropertyFeature.AutoDeleteWhenFileUpdate);
        Assert.AreEqual(
            (await _singletonFileSystem.GetProperty(testFile, "delete-on-update"))!.Value.ToArray(),
            Encoding.UTF8.GetBytes("123"));
        await _singletonFileSystem.WriteFile(testFile, new ReadOnlyMemory<byte>(new byte[] { 1 }));
        Assert.Null(await _singletonFileSystem.GetProperty(testFile, "delete-on-update"));
    }
}
