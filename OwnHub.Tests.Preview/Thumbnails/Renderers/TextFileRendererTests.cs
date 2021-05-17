using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using OwnHub.FileSystem;
using OwnHub.FileSystem.Provider;
using OwnHub.Preview.Thumbnails;
using OwnHub.Preview.Thumbnails.Renderers;
using OwnHub.Utils;

namespace OwnHub.Tests.Preview.Thumbnails.Renderers
{
    public class TextFileRendererTests
    {
        [Test]
        public async Task TestRenderTextFileIcon()
        {
            var renderContext = new ThumbnailsRenderContext();
            var fileSystem = new VirtualFileSystemService();
            fileSystem.RegisterFileSystemProvider(
                "test",
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));
            var renderer = new TextFileRenderer(fileSystem);

            var renderOption = new ThumbnailsRenderOption(Url.Parse("file://test/Resources/Test Text.txt"))
            {
                FileType = FileType.File, MimeType = "text/plain"
            };

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, renderOption with { Size = 1024 });
            await renderContext.SaveTestResult("1024w");

            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, renderOption with { Size = 512 });
            await renderContext.SaveTestResult("512w");

            renderContext.Resize(256, 256, false);
            await renderer.Render(renderContext, renderOption with { Size = 256 });
            await renderContext.SaveTestResult("256w");

            renderContext.Resize(128, 128, false);
            await renderer.Render(renderContext, renderOption with { Size = 128 });
            await renderContext.SaveTestResult("128w");

            renderContext.Resize(64, 64, false);
            await renderer.Render(renderContext, renderOption with { Size = 64 });
            await renderContext.SaveTestResult("64w");
        }

        [Test]
        public async Task TestRenderFormattedTextFileIcon()
        {
            var renderContext = new ThumbnailsRenderContext();
            var fileSystem = new VirtualFileSystemService();
            fileSystem.RegisterFileSystemProvider(
                "test",
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));
            var renderer = new TextFileRenderer(fileSystem);

            var renderOption = new ThumbnailsRenderOption(Url.Parse("file://test/Resources/Program.c"))
            {
                FileType = FileType.File, MimeType = "text/x-csrc", Size = 1024
            };

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, renderOption);
            await renderContext.SaveTestResult("1024w");
        }
    }
}
