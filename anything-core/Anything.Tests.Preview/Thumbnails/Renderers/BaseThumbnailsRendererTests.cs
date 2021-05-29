using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails.Renderers
{
    public class BaseThumbnailsRendererTests
    {
        [Test]
        public async Task ImplementationTest()
        {
            IThumbnailsRenderer testRenderer = new TestRenderer();
            Assert.IsTrue(
                testRenderer.IsSupported(
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.txt"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        "text/plain")));
            Assert.IsTrue(
                testRenderer.IsSupported(
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.png"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        "image/png")));
            Assert.IsFalse(
                testRenderer.IsSupported(
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.png"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 1001, FileType.File),
                        "image/png")));
            Assert.IsFalse(
                testRenderer.IsSupported(
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.jpg"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        "image/jpeg")));

            var testRenderContext = new ThumbnailsRenderContext();

            Assert.IsTrue(
                await testRenderer.Render(
                    testRenderContext,
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.png"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        "image/png"),
                    new ThumbnailsRenderOption()));
            Assert.IsFalse(
                await testRenderer.Render(
                    testRenderContext,
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.jpg"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        "image/jpeg"),
                    new ThumbnailsRenderOption()));
        }

        private class TestRenderer : BaseThumbnailsRenderer
        {
            protected override long MaxFileSize => 1000;

            protected override string[] SupportMimeTypes { get; } = { "text/plain", "image/png" };

            protected override Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderFileInfo fileInfo, ThumbnailsRenderOption option)
            {
                Assert.AreEqual("image/png", fileInfo.MimeType);
                return Task.FromResult(true);
            }
        }
    }
}
