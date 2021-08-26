using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
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
            using var fileService = new FileService();
            fileService.AddTestFileSystem(
                Url.Parse("file://test/"),
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(FFmpegRendererTests).Assembly)));
            IThumbnailsRenderer renderer = new FFmpegRenderer(fileService);

            async ValueTask<ThumbnailsRenderFileInfo> MakeFileInfo(
                IFileService fs,
                string filename,
                MimeType? mimeType = null)
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new ThumbnailsRenderFileInfo(
                    url,
                    await fs.Stat(url),
                    mimeType ?? MimeType.video_mp4);
            }

            var renderOption = new ThumbnailsRenderOption();
            renderContext.Resize(512, 512, false);
            await renderer.Render(renderContext, await MakeFileInfo(fileService, "miku.mp4"), renderOption with { Size = 512 });
            await renderContext.SaveTestResult("512w");
        }
    }
}
