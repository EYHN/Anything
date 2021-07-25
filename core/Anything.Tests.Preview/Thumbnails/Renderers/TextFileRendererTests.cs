﻿using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails.Renderers
{
    public class TextFileRendererTests
    {
        [Test]
        public async Task TestRenderTextFileIcon()
        {
            using var renderContext = new ThumbnailsRenderContext();
            using var fileService = new EmbeddedFileService(Url.Parse("file://test/"), typeof(TextFileRendererTests).Assembly);
            IThumbnailsRenderer renderer = new TextFileRenderer(fileService);

            async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(IFileService fs, string filename, string mimeType = "text/plain")
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new ThumbnailsRenderFileInfo(url, await fs.Stat(url), mimeType);
            }

            var renderOption = new ThumbnailsRenderOption();

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, await MakeFileInfo(fileService, "Test Text.txt"), renderOption with { Size = 1024 });
            await renderContext.SaveTestResult("1024w");

            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, await MakeFileInfo(fileService, "Test Text.txt"), renderOption with { Size = 512 });
            await renderContext.SaveTestResult("512w");

            renderContext.Resize(256, 256, false);
            await renderer.Render(renderContext, await MakeFileInfo(fileService, "Test Text.txt"), renderOption with { Size = 256 });
            await renderContext.SaveTestResult("256w");

            renderContext.Resize(128, 128, false);
            await renderer.Render(renderContext, await MakeFileInfo(fileService, "Test Text.txt"), renderOption with { Size = 128 });
            await renderContext.SaveTestResult("128w");

            renderContext.Resize(64, 64, false);
            await renderer.Render(renderContext, await MakeFileInfo(fileService, "Test Text.txt"), renderOption with { Size = 64 });
            await renderContext.SaveTestResult("64w");
        }

        [Test]
        public async Task TestRenderFormattedTextFileIcon()
        {
            using var renderContext = new ThumbnailsRenderContext();
            using var fileService = new EmbeddedFileService(Url.Parse("file://test/"), typeof(TextFileRendererTests).Assembly);
            IThumbnailsRenderer renderer = new TextFileRenderer(fileService);

            async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(string filename, string mimeType = "text/plain")
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new ThumbnailsRenderFileInfo(url, await fileService.Stat(url), mimeType);
            }

            var renderOption = new ThumbnailsRenderOption { Size = 1024 };

            renderContext.Resize(1024, 1024, false);
            await renderer.Render(renderContext, await MakeFileInfo("Program.c"), renderOption);
            await renderContext.SaveTestResult("1024w");
        }
    }
}
