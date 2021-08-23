﻿using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Mime.Schema;
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
                        MimeType.text_plain)));
            Assert.IsTrue(
                testRenderer.IsSupported(
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.png"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        MimeType.image_png)));
            Assert.IsFalse(
                testRenderer.IsSupported(
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.png"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 1001, FileType.File),
                        MimeType.image_png)));
            Assert.IsFalse(
                testRenderer.IsSupported(
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.jpg"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        MimeType.image_jpeg)));

            using var testRenderContext = new ThumbnailsRenderContext();

            Assert.IsTrue(
                await testRenderer.Render(
                    testRenderContext,
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.png"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        MimeType.image_png),
                    new ThumbnailsRenderOption()));
            Assert.IsFalse(
                await testRenderer.Render(
                    testRenderContext,
                    new ThumbnailsRenderFileInfo(
                        Url.Parse("file://test/a.jpg"),
                        new FileStats(DateTimeOffset.Now, DateTimeOffset.Now, 100, FileType.File),
                        MimeType.image_jpeg),
                    new ThumbnailsRenderOption()));
        }

        private class TestRenderer : BaseThumbnailsRenderer
        {
            protected override long MaxFileSize => 1000;

            protected override ImmutableArray<MimeType> SupportMimeTypes { get; } =
                new[] { MimeType.text_plain, MimeType.image_png }
                    .ToImmutableArray();

            protected override Task<bool> Render(
                ThumbnailsRenderContext ctx,
                ThumbnailsRenderFileInfo fileInfo,
                ThumbnailsRenderOption option)
            {
                Assert.AreEqual(MimeType.image_png, fileInfo.MimeType);
                return Task.FromResult(true);
            }
        }
    }
}
