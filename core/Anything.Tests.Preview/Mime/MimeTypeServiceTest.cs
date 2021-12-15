using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.Preview;
using Anything.Preview.Mime;
using Anything.Preview.Mime.Schema;
using Anything.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Mime;

public class MimeTypeServiceTest
{
    [Test]
    public async Task FeatureTest()
    {
        var services = new ServiceCollection();
        services.TryAddSingletonFileService(builder =>
            builder.TryAddEmbeddedFileSystem("test", new EmbeddedFileProvider(typeof(MimeTypeServiceTest).Assembly)));
        services.TryAddMimeTypeFeature();
        services.AddTestLogging();
        await using var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        var mimeTypeService = serviceProvider.GetRequiredService<IMimeTypeService>();

        var testImage = await fileService.CreateFileHandle(Url.Parse("file://test/Resources/Test Image.png"));
        Assert.AreEqual(
            MimeType.image_png,
            await mimeTypeService.GetMimeType(testImage));
    }
}
