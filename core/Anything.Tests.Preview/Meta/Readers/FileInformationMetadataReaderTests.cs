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

public class FileInformationMetadataReaderTests
{
    [Test]
    public async Task ReaderTest()
    {
        var services = new ServiceCollection();
        services.TryAddSingletonFileService(builder =>
            builder.TryAddEmbeddedFileSystem("test", new EmbeddedFileProvider(typeof(FileInformationMetadataReaderTests).Assembly)));
        services.AddTestLogging();
        await using var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        async ValueTask<MetadataReaderFileInfo> MakeFileInfo(IFileService fs, string filename)
        {
            var fileHandle = await fs.CreateFileHandle(Url.Parse("file://test/Resources/" + filename));
            return new MetadataReaderFileInfo(fileHandle, await fs.Stat(fileHandle), MimeType.text_plain);
        }

        IMetadataReader reader = new FileInformationMetadataReader();
        var metadata = await reader.ReadMetadata(
            new Metadata(),
            await MakeFileInfo(fileService, "Test Text.txt"),
            new MetadataReaderOption());
        Console.WriteLine(metadata.ToString(true));
    }
}
