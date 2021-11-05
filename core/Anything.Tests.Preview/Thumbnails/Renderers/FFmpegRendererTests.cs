using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Preview.Mime.Schema;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails.Renderers
{
    public class FFmpegRendererTests
    {
        [Test]
        public async Task TestRenderVideoIcon()
        {
            using var renderContext = new ThumbnailsRenderContext();
            using var fileService = new FileService(TestUtils.Logger);
            fileService.AddFileSystem(
                "test",
                new EmbeddedFileSystem(new EmbeddedFileProvider(typeof(FFmpegRendererTests).Assembly)));
            IThumbnailsRenderer renderer = new FFmpegRenderer(fileService);

            async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(
                IFileService fs,
                string filename,
                MimeType? mimeType = null)
            {
                var fileHandle = await fs.CreateFileHandle(Url.Parse("file://test/Resources/" + filename));
                return new ThumbnailsRenderFileInfo(
                    fileHandle,
                    await fs.Stat(fileHandle),
                    mimeType ?? MimeType.video_mp4);
            }

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
    }
}
