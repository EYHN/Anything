﻿using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Tags;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Tags
{
    public class TagServiceTests
    {
        [Test]
        public async Task FeatureTests()
        {
            using var fileService = new FileService();
            fileService.AddFileSystem("test", new MemoryFileSystem());
            using var memoryService = new TagService.MemoryStorage();
            using var service = new TagService(fileService, memoryService);

            var root = await fileService.CreateFileHandle(Url.Parse("file://test/"));
            var testFile = await fileService.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

            Assert.AreEqual(Array.Empty<Tag>(), await service.GetTags(testFile));

            await service.AddTags(testFile, new Tag[] { new("foo"), new("bar") });
            Assert.AreEqual(new Tag[] { new("foo"), new("bar") }, await service.GetTags(testFile));

            await service.AddTags(testFile, new Tag[] { new("append") });
            Assert.AreEqual(new Tag[] { new("foo"), new("bar"), new("append") }, await service.GetTags(testFile));

            await service.RemoveTags(testFile, new Tag[] { new("bar") });
            Assert.AreEqual(new Tag[] { new("foo"), new("append") }, await service.GetTags(testFile));

            // delete test
            await fileService.Delete(testFile, root, "test_file", false);
            await fileService.WaitComplete();

            Assert.AreEqual(Array.Empty<Tag>(), await service.GetTags(testFile));
        }
    }
}
