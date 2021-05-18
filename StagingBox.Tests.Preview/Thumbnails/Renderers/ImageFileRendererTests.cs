using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using StagingBox.FileSystem;
using StagingBox.FileSystem.Provider;
using StagingBox.Preview.Thumbnails;
using StagingBox.Preview.Thumbnails.Renderers;
using StagingBox.Utils;

namespace StagingBox.Tests.Preview.Thumbnails.Renderers
{
    [TestFixture]
    public class ImageFileRendererTests
    {
        [Test]
        public async Task TestRenderImageIcon()
        {
            var renderContext = new ThumbnailsRenderContext();
            var fileSystem = new VirtualFileSystemService();
            fileSystem.RegisterFileSystemProvider(
                "test",
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));
            var renderer = new ImageFileRenderer(fileSystem);

            var renderOption = new ThumbnailsRenderOption(Url.Parse("file://test/Resources/Test Image.png"))
            {
                FileType = FileType.File, MimeType = "image/png"
            };

            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, renderOption with { Size = 512 });
            await renderContext.SaveTestResult("512w");

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, renderOption with { Size = 1024 });
            await renderContext.SaveTestResult("1024w");

            renderContext.Resize(256, 256, false);
            await renderer.Render(renderContext, renderOption with { Size = 256 });
            await renderContext.SaveTestResult("256w");

            renderContext.Resize(128, 128, false);
            await renderer.Render(renderContext, renderOption with { Size = 128 });
            await renderContext.SaveTestResult("128w");

            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, renderOption with { Url = Url.Parse("file://test/Resources/20000px.png"), Size = 512 });
            await renderContext.SaveTestResult("Large Pixels");

            await renderer.Render(
                renderContext,
                renderOption with
                {
                    Url = Url.Parse("file://test/Resources/EXIF Orientation Tag.jpg"), MimeType = "image/jpeg", Size = 512
                });
            await renderContext.SaveTestResult("EXIF Orientation Tag");

            await renderer.Render(
                renderContext,
                renderOption with
                {
                    Url = Url.Parse("file://test/Resources/Grayscale With Alpha.png"), MimeType = "image/png", Size = 512
                });
            await renderContext.SaveTestResult("Grayscale With Alpha");

            await renderer.Render(
                renderContext,
                renderOption with { Url = Url.Parse("file://test/Resources/Grayscale.jpg"), MimeType = "image/jpeg", Size = 512 });
            await renderContext.SaveTestResult("Grayscale");
        }
    }
}
