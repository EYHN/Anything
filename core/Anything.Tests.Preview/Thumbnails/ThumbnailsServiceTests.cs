using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Extensions;
using Anything.FileSystem.Singleton;
using Anything.Preview;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails;

public class ThumbnailsServiceTests
{
    private readonly ServiceCollection _services = new();

    [OneTimeSetUp]
    public void Setup()
    {
        _services.TryAddSingletonFileService(builder =>
            builder.TryAddEmbeddedFileSystem("test", new EmbeddedFileProvider(typeof(ThumbnailsServiceTests).Assembly))
                .TryAddMemoryFileSystem("memory"));
        _services.AddTestLogging();
        _services.TryAddMimeTypeFeature();
        _services.TryAddThumbnailsFeature(false);
        _services.TryAddThumbnailsRenderer<TestImageFileRenderer>();
    }

    [Test]
    public async Task FeatureTest()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        var thumbnailsService = serviceProvider.GetRequiredService<IThumbnailsService>();

        var testImageFileHandle = await fileService.CreateFileHandle(Url.Parse("file://test/Resources/Test Image.png"));

        var thumbnail = await thumbnailsService.GetThumbnailImage(
            testImageFileHandle,
            new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
        Assert.AreEqual(256, thumbnail!.Size);
        Assert.AreEqual("image/png", thumbnail.ImageFormat);
        await TestUtils.SaveResult("256w.png", thumbnail.Data);

        thumbnail = await thumbnailsService.GetThumbnailImage(
            testImageFileHandle,
            new ThumbnailOption { Size = 128, ImageFormat = "image/png" });
        Assert.AreEqual(128, thumbnail!.Size);
        Assert.AreEqual("image/png", thumbnail.ImageFormat);
        await TestUtils.SaveResult("128w.png", thumbnail.Data);

        thumbnail = await thumbnailsService.GetThumbnailImage(
            testImageFileHandle,
            new ThumbnailOption { Size = 512, ImageFormat = "image/png" });
        Assert.AreEqual(512, thumbnail!.Size);
        Assert.AreEqual("image/png", thumbnail.ImageFormat);
        await TestUtils.SaveResult("512w.png", thumbnail.Data);
    }

    [Test]
    public async Task CacheTest()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        var thumbnailsService = serviceProvider.GetRequiredService<IThumbnailsService>();

        await fileService.CopyFile(
            await fileService.CreateFileHandle(Url.Parse("file://test/Resources/Test Image.png")),
            await fileService.CreateFileHandle(Url.Parse("file://memory/")),
            "Test Image.png");

        var testImageFileHandle = await fileService.CreateFileHandle(Url.Parse("file://memory/Test Image.png"));

        TestImageFileRenderer.RenderCount = 0;

        var thumbnail = await thumbnailsService.GetThumbnailImage(
            testImageFileHandle,
            new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
        Assert.AreEqual(256, thumbnail!.Size);
        Assert.AreEqual("image/png", thumbnail.ImageFormat);
        await TestUtils.SaveResult("256w.png", thumbnail.Data);

        Assert.AreEqual(1, TestImageFileRenderer.RenderCount);

        thumbnail = await thumbnailsService.GetThumbnailImage(
            testImageFileHandle,
            new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
        Assert.AreEqual(256, thumbnail!.Size);
        Assert.AreEqual("image/png", thumbnail.ImageFormat);
        await TestUtils.SaveResult("256w-2.png", thumbnail.Data);

        Assert.AreEqual(1, TestImageFileRenderer.RenderCount);

        await fileService.WriteFile(
            testImageFileHandle,
            Resources.ReadEmbeddedFile(typeof(ThumbnailsServiceTests).Assembly, "/Resources/Transparent.png"));

        thumbnail = await thumbnailsService.GetThumbnailImage(
            testImageFileHandle,
            new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
        Assert.AreEqual(256, thumbnail!.Size);
        Assert.AreEqual("image/png", thumbnail.ImageFormat);
        await TestUtils.SaveResult("256w.png", thumbnail.Data);

        Assert.AreEqual(2, TestImageFileRenderer.RenderCount);
    }

    private class TestImageFileRenderer : IThumbnailsRenderer
    {
        private readonly IThumbnailsRenderer _wrappedThumbnailsRenderer;

        public TestImageFileRenderer(IFileService fileService)
        {
            _wrappedThumbnailsRenderer = new ImageFileRenderer(fileService);
        }

        public static int RenderCount { get; set; }

        public Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderFileInfo fileInfo, ThumbnailsRenderOption option)
        {
            RenderCount++;
            return _wrappedThumbnailsRenderer.Render(ctx, fileInfo, option);
        }

        public bool IsSupported(ThumbnailsRenderFileInfo fileInfo)
        {
            return _wrappedThumbnailsRenderer.IsSupported(fileInfo);
        }
    }
}
