using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails.Renderers
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
            IThumbnailsRenderer renderer = new TextFileRenderer(fileSystem);

            async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(string filename, string mimeType = "text/plain")
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new ThumbnailsRenderFileInfo(url, await fileSystem!.Stat(url), mimeType);
            }

            var renderOption = new ThumbnailsRenderOption();

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Text.txt"), renderOption with { Size = 1024 });
            await renderContext.SaveTestResult("1024w");

            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Text.txt"), renderOption with { Size = 512 });
            await renderContext.SaveTestResult("512w");

            renderContext.Resize(256, 256, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Text.txt"), renderOption with { Size = 256 });
            await renderContext.SaveTestResult("256w");

            renderContext.Resize(128, 128, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Text.txt"), renderOption with { Size = 128 });
            await renderContext.SaveTestResult("128w");

            renderContext.Resize(64, 64, false);
            await renderer.Render(renderContext, await MakeFileInfo("Test Text.txt"), renderOption with { Size = 64 });
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
            IThumbnailsRenderer renderer = new TextFileRenderer(fileSystem);

            async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(string filename, string mimeType = "text/plain")
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new ThumbnailsRenderFileInfo(url, await fileSystem!.Stat(url), mimeType);
            }

            var renderOption = new ThumbnailsRenderOption { Size = 1024 };

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, await MakeFileInfo("Program.c"), renderOption);
            await renderContext.SaveTestResult("1024w");
        }
    }
}
