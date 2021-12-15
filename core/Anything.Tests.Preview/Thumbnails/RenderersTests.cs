using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.Preview.Mime.Schema;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails;

public class RenderersTests
{
    private readonly ServiceCollection _services = new();

    [OneTimeSetUp]
    public void Setup()
    {
        _services.TryAddSingletonFileService(builder =>
            builder.TryAddEmbeddedFileSystem("test", new EmbeddedFileProvider(typeof(RenderersTests).Assembly)));
        _services.AddTestLogging();
    }

    private static async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(
        IFileService fs,
        string filename,
        MimeType mimeType)
    {
        var fileHandle = await fs.CreateFileHandle(Url.Parse("file://test/Resources/" + filename));
        return new ThumbnailsRenderFileInfo(
            fileHandle,
            await fs.Stat(fileHandle),
            mimeType);
    }

    [Test]
    public async Task TestRenderVideoIcon()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        using var renderContext = new ThumbnailsRenderContext();

        IThumbnailsRenderer renderer = new FFmpegRenderer(fileService);

        var renderOption = new ThumbnailsRenderOption();
        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/example.mp4", MimeType.video_mp4),
            renderOption);
        await renderContext.SaveTestResult("example-mp4");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/example.mov", MimeType.video_quicktime),
            renderOption);
        await renderContext.SaveTestResult("example-mov");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/example.avi", MimeType.video_x_msvideo),
            renderOption);
        await renderContext.SaveTestResult("example-avi");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/example.ogv", MimeType.video_ogg),
            renderOption);
        await renderContext.SaveTestResult("example-ogv");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/example.webm", MimeType.video_webm),
            renderOption);
        await renderContext.SaveTestResult("example-webm");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/example.wmv", MimeType.video_x_ms_wmv),
            renderOption);
        await renderContext.SaveTestResult("example-wmv");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/example.wmv", MimeType.video_x_ms_wmv),
            renderOption);
        await renderContext.SaveTestResult("example-wmv");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "videos/narrow.mp4", MimeType.video_mp4),
            renderOption);
        await renderContext.SaveTestResult("narrow");

        renderContext.Resize(512, 512, false);
        Assert.CatchAsync(async () =>
        {
            await renderer.Render(
                renderContext,
                await MakeFileInfo(fileService, "videos/empty.mp4", MimeType.video_x_ms_wmv),
                renderOption);
        });
    }

    [Test]
    public async Task TestRenderImageIcon()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        using var renderContext = new ThumbnailsRenderContext();

        IThumbnailsRenderer renderer = new ImageFileRenderer(fileService);

        var renderOption = new ThumbnailsRenderOption();
        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Image.png", MimeType.image_png),
            renderOption with { Size = 512 });
        await renderContext.SaveTestResult("512w");

        renderContext.Resize(1024, 1024, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Image.png", MimeType.image_png),
            renderOption with { Size = 1024 });
        await renderContext.SaveTestResult("1024w");

        renderContext.Resize(256, 256, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Image.png", MimeType.image_png),
            renderOption with { Size = 256 });
        await renderContext.SaveTestResult("256w");

        renderContext.Resize(128, 128, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Image.png", MimeType.image_png),
            renderOption with { Size = 128 });
        await renderContext.SaveTestResult("128w");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "20000px.png", MimeType.image_png),
            renderOption with { Size = 512 });
        await renderContext.SaveTestResult("Large Pixels");

        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "EXIF Orientation Tag.jpg", MimeType.image_jpeg),
            renderOption with { Size = 512 });
        await renderContext.SaveTestResult("EXIF Orientation Tag");

        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Grayscale With Alpha.png", MimeType.image_png),
            renderOption with { Size = 512 });
        await renderContext.SaveTestResult("Grayscale With Alpha");

        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Grayscale.jpg", MimeType.image_jpeg),
            renderOption with { Size = 512 });
        await renderContext.SaveTestResult("Grayscale");

        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Pdf Sample.pdf", MimeType.application_pdf),
            renderOption with { Size = 512 });
        await renderContext.SaveTestResult("Pdf Sample");
    }

    [Test]
    public async Task TestRenderTextFileIcon()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        using var renderContext = new ThumbnailsRenderContext();

        IThumbnailsRenderer renderer = new TextFileRenderer(fileService);

        var renderOption = new ThumbnailsRenderOption();

        renderContext.Resize(1024, 1024, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Text.txt", MimeType.text_plain),
            renderOption with { Size = 1024 });
        await renderContext.SaveTestResult("1024w");

        renderContext.Resize(512, 512, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Text.txt", MimeType.text_plain),
            renderOption with { Size = 512 });
        await renderContext.SaveTestResult("512w");

        renderContext.Resize(256, 256, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Text.txt", MimeType.text_plain),
            renderOption with { Size = 256 });
        await renderContext.SaveTestResult("256w");

        renderContext.Resize(128, 128, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Text.txt", MimeType.text_plain),
            renderOption with { Size = 128 });
        await renderContext.SaveTestResult("128w");

        renderContext.Resize(64, 64, false);
        await renderer.Render(
            renderContext,
            await MakeFileInfo(fileService, "Test Text.txt", MimeType.text_plain),
            renderOption with { Size = 64 });
        await renderContext.SaveTestResult("64w");
    }

    [Test]
    public async Task TestRenderFormattedTextFileIcon()
    {
        await using var serviceProvider = _services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        using var renderContext = new ThumbnailsRenderContext();

        IThumbnailsRenderer renderer = new TextFileRenderer(fileService);

        var renderOption = new ThumbnailsRenderOption { Size = 1024 };

        renderContext.Resize(1024, 1024, false);
        await renderer.Render(renderContext, await MakeFileInfo(fileService, "Program.c", MimeType.text_x_c), renderOption);
        await renderContext.SaveTestResult("1024w");
    }
}
