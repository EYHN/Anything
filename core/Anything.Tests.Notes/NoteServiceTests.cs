using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Notes;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Notes
{
    public class NoteServiceTests
    {
        [Test]
        public async Task FeatureTests()
        {
            using var fileService = new FileService(TestUtils.Logger);
            fileService.AddFileSystem("test", new MemoryFileSystem());
            using var memoryService = new NoteService.MemoryStorage();
            using var service = new NoteService(fileService, memoryService, TestUtils.Logger);

            var root = await fileService.CreateFileHandle(Url.Parse("file://test/"));
            var testFile = await fileService.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

            Assert.AreEqual(string.Empty, await service.GetNotes(testFile));

            await service.SetNotes(testFile, "foo");
            Assert.AreEqual("foo", await service.GetNotes(testFile));

            await service.SetNotes(testFile, "bar");
            Assert.AreEqual("bar", await service.GetNotes(testFile));

            await service.SetNotes(testFile, "");
            Assert.AreEqual("", await service.GetNotes(testFile));
        }
    }
}
