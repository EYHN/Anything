using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails.Renderers
{
    [TestFixture]
    public class ImageFileRendererTests
    {
        [Test]
        public async Task TestRenderImageIcon()
        {
            var renderContext = new ThumbnailsRenderContext();
            var fileService = FileServiceFactory.BuildEmbeddedFileService(typeof(ImageFileRendererTests).Assembly);
            IThumbnailsRenderer renderer = new ImageFileRenderer(fileService);

            async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(string filename, string mimeType = "image/png")
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new ThumbnailsRenderFileInfo(url, await fileService.Stat(url), mimeType);
            }

            var renderOption = new ThumbnailsRenderOption();
            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Image.png"), renderOption with { Size = 512 });
            await renderContext.SaveTestResult("512w");

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Image.png"), renderOption with { Size = 1024 });
            await renderContext.SaveTestResult("1024w");

            renderContext.Resize(256, 256, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Image.png"), renderOption with { Size = 256 });
            await renderContext.SaveTestResult("256w");

            renderContext.Resize(128, 128, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Image.png"), renderOption with { Size = 128 });
            await renderContext.SaveTestResult("128w");

            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, await MakeFileInfo("20000px.png"), renderOption with { Size = 512 });
            await renderContext.SaveTestResult("Large Pixels");

            await renderer.Render(
                renderContext,
                await MakeFileInfo("EXIF Orientation Tag.jpg", "image/jpeg"),
                renderOption with { Size = 512 });
            await renderContext.SaveTestResult("EXIF Orientation Tag");

            await renderer.Render(
                renderContext,
                await MakeFileInfo("Grayscale With Alpha.png"),
                renderOption with { Size = 512 });
            await renderContext.SaveTestResult("Grayscale With Alpha");

            await renderer.Render(
                renderContext,
                await MakeFileInfo("Grayscale.jpg", "image/jpeg"),
                renderOption with { Size = 512 });
            await renderContext.SaveTestResult("Grayscale");

            await renderer.Render(
                renderContext,
                await MakeFileInfo("Pdf Sample.pdf", "application/pdf"),
                renderOption with { Size = 512 });
            await renderContext.SaveTestResult("Pdf Sample");
        }
    }
}
