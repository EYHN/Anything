using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.Notes;
using Anything.Utils;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Anything.Tests.Notes;

public class NoteServiceTests
{
    [Test]
    public async Task FeatureTests()
    {
        var services = new ServiceCollection();
        services.TryAddSingletonFileService(builder => builder.TryAddMemoryFileSystem("test"));
        services.TryAddNoteFeature();
        services.AddTestLogging();
        await using var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        var noteService = serviceProvider.GetRequiredService<INoteService>();

        var root = await fileService.CreateFileHandle(Url.Parse("file://test/"));
        var testFile = await fileService.CreateFile(root, "test_file", ReadOnlyMemory<byte>.Empty);

        Assert.AreEqual(string.Empty, await noteService.GetNotes(testFile));

        await noteService.SetNotes(testFile, "foo");
        Assert.AreEqual("foo", await noteService.GetNotes(testFile));

        await noteService.SetNotes(testFile, "bar");
        Assert.AreEqual("bar", await noteService.GetNotes(testFile));

        await noteService.SetNotes(testFile, "");
        Assert.AreEqual("", await noteService.GetNotes(testFile));
    }
}
