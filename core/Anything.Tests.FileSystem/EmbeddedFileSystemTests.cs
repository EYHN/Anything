using System.IO;
using System.Text;
using System.Threading.Tasks;
using Anything.FileSystem.Singleton.Impl;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.FileSystem;

public class EmbeddedFileSystemTests
{
    [Test]
    public async Task ReadEmbeddedFileTest()
    {
        var embeddedFileSystem = new EmbeddedFileSystem(new EmbeddedFileProvider(typeof(EmbeddedFileSystemTests).Assembly));
        var testFile = await embeddedFileSystem.CreateFileHandle(Url.Parse("file://test/Resources/test_directory/test_file"));
        Assert.AreEqual(
            "12345",
            Encoding.UTF8.GetString(
                (await embeddedFileSystem.ReadFile(testFile))
                .ToArray()));
        Assert.AreEqual(
            5,
            (await embeddedFileSystem.Stat(testFile)).Size);
        Assert.IsTrue(await embeddedFileSystem.ReadFileStream(testFile, async stream =>
        {
            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            Assert.AreEqual("12345", Encoding.UTF8.GetString(memoryStream.ToArray()));
            return true;
        }));
    }
}
