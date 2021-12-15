using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.Tags;
using Anything.Utils;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Anything.Tests.Tags;

public class TagServiceTests
{
    [Test]
    public async Task FeatureTests()
    {
        var services = new ServiceCollection();
        services.TryAddSingletonFileService(builder => builder.TryAddMemoryFileSystem("test"));
        services.AddTestLogging();
        services.TryAddTagFeature();
        await using var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        var tagService = serviceProvider.GetRequiredService<ITagService>();

        var root = await fileService.CreateFileHandle(Url.Parse("file://test/"));
        var testFile = await fileService.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

        Assert.AreEqual(Array.Empty<Tag>(), await tagService.GetTags(testFile));

        await tagService.SetTags(testFile, new Tag[] { new("foo"), new("bar") });
        Assert.AreEqual(new Tag[] { new("foo"), new("bar") }, await tagService.GetTags(testFile));

        await tagService.SetTags(testFile, new Tag[] { new("foo") });
        Assert.AreEqual(new Tag[] { new("foo") }, await tagService.GetTags(testFile));
    }
}
