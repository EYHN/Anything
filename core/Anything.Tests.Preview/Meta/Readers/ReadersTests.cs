using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.Preview.Meta.Readers;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime.Schema;
using Anything.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Meta.Readers;

public class ReadersTests
{
    private readonly ServiceCollection _services = new();

    [OneTimeSetUp]
    public void Setup()
    {
        _services.TryAddSingletonFileService(builder =>
            builder.TryAddEmbeddedFileSystem("test", new EmbeddedFileProvider(typeof(ReadersTests).Assembly)));
        _services.AddTestLogging();
    }

    private static async ValueTask<MetadataReaderFileInfo> MakeFileInfo(
        IFileService fs,
        string filename,
        MimeType mimeType)
    {
        var fileHandle = await fs.CreateFileHandle(Url.Parse("file://test/Resources/" + filename));
        return new MetadataReaderFileInfo(fileHandle, await fs.Stat(fileHandle), mimeType);
    }

    [Test]
    public async Task ImageFileMetadataReaderTest()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        IMetadataReader reader = new ImageFileMetadataReader(fileService);
        Console.WriteLine(
            (await reader.ReadMetadata(
                new Metadata(),
                await MakeFileInfo(fileService, "Test Image.png", MimeType.image_png),
                new MetadataReaderOption()))
            .ToString(true));
        Console.WriteLine(
            (await reader.ReadMetadata(
                new Metadata(),
                await MakeFileInfo(fileService, "Sony ILCE-7M3 (A7M3).jpg", MimeType.image_jpeg),
                new MetadataReaderOption()))
            .ToString(true));
        Console.WriteLine(
            (await reader.ReadMetadata(
                new Metadata(),
                await MakeFileInfo(fileService, "Test WebP.webp", MimeType.image_webp),
                new MetadataReaderOption()))
            .ToString(true));
    }

    [Test]
    public async Task FileInformationMetadataReaderTest()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        IMetadataReader reader = new FileInformationMetadataReader();
        Console.WriteLine((await reader.ReadMetadata(
            new Metadata(),
            await MakeFileInfo(fileService, "Test Text.txt", MimeType.text_plain),
            new MetadataReaderOption())).ToString(true));
    }

    [Test]
    public async Task AudioFileMetadataReaderTest()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        IMetadataReader reader = new AudioFileMetadataReader(fileService);
        Console.WriteLine((await reader.ReadMetadata(
            new Metadata(),
            await MakeFileInfo(fileService, "Test Audio.mp3", MimeType.audio_mpeg),
            new MetadataReaderOption())).ToString(true));

        Console.WriteLine((await reader.ReadMetadata(
            new Metadata(),
            await MakeFileInfo(fileService, "Test Music.mp3", MimeType.audio_mpeg),
            new MetadataReaderOption())).ToString(true));
    }
}
