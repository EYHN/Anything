using System;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Fork;
using Anything.Utils;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Anything.Tests.Fork
{
    public class EfCoreFileForkContextTests
    {
        [Test]
        public async Task FeatureTest()
        {
            using var fileService = new FileService();
            fileService.AddFileSystem(
                "test",
                new MemoryFileSystem());
            var root = await fileService.CreateFileHandle(Url.Parse("file://test/"));
            var testFile = await fileService.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

            using var storage = new EfCoreFileForkService.MemoryStorage();
            using var forkService =
                new EfCoreFileForkService(fileService, "test_fork", storage, typeof(TestFork));

            // insert
            await using (var forkContext = forkService.CreateContext())
            {
                var testFork = new TestFork { Message = "Hello World", File = await forkContext.GetOrCreateFileEntity(testFile) };
                await forkContext.Set<TestFork>().AddAsync(testFork);
                await forkContext.SaveChangesAsync();
            }

            // query
            await using (var forkContext = forkService.CreateContext())
            {
                var result = await forkContext.Set<TestFork>().AsQueryable().Where(f => f.File.FileHandle == testFile).ToListAsync();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("Hello World", result[0].Message);
            }

            await fileService.Delete(testFile, root, "test_file", false);
            await fileService.WaitComplete();
            await using (var forkContext = forkService.CreateContext())
            {
                var count = await forkContext.Set<TestFork>().AsQueryable().CountAsync();
                Assert.AreEqual(0, count);
            }
        }

        private class TestFork : EfCoreFileForkService.FileForkEntity
        {
            public int Id { get; set; }

            public string Message { get; set; } = null!;
        }
    }
}
