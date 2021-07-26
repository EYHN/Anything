using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Cache;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails
{
    public class ThumbnailsServiceTests
    {
        [Test]
        public async Task FeatureTest()
        {
            using var fileService = new FileService();
            fileService.AddTestFileSystem(
                Url.Parse("file://test/"),
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(ThumbnailsServiceTests).Assembly)));
            using var sqliteContext = TestUtils.CreateSqliteContext();

            var thumbnailsService = new ThumbnailsService(
                fileService,
                new MimeTypeService(MimeTypeRules.TestRules),
                new ThumbnailsCacheDatabaseStorage(sqliteContext));
            var imageFileRenderer = new ImageFileRenderer(fileService);
            var testFileRenderer = new TestImageFileRenderer(imageFileRenderer);
            thumbnailsService.RegisterRenderer(testFileRenderer);

            var thumbnail = await thumbnailsService.GetThumbnail(
                Url.Parse("file://test/Resources/Test Image.png"),
                new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await TestUtils.SaveResult("256w.png", thumbnail.GetStream());
            Assert.AreEqual(1, testFileRenderer.RenderCount);

            // cache test
            thumbnail = await thumbnailsService.GetThumbnail(
                Url.Parse("file://test/Resources/Test Image.png"),
                new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await TestUtils.SaveResult("256w - form cache.png", thumbnail.GetStream());
            Assert.AreEqual(1, testFileRenderer.RenderCount);

            thumbnail = await thumbnailsService.GetThumbnail(
                Url.Parse("file://test/Resources/Test Image.png"),
                new ThumbnailOption { Size = 128, ImageFormat = "image/png" });
            Assert.AreEqual(128, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await TestUtils.SaveResult("128w.png", thumbnail.GetStream());
            Assert.AreEqual(1, testFileRenderer.RenderCount);

            thumbnail = await thumbnailsService.GetThumbnail(
                Url.Parse("file://test/Resources/Test Image.png"),
                new ThumbnailOption { Size = 512, ImageFormat = "image/png" });
            Assert.AreEqual(512, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await TestUtils.SaveResult("512w.png", thumbnail.GetStream());
            Assert.AreEqual(2, testFileRenderer.RenderCount);
        }

        [Test]
        public async Task CacheAutoCleanUpTest()
        {
            using var fileService = new FileService();
            fileService.AddTestFileSystem(
                Url.Parse("file://test/"),
                new MemoryFileSystemProvider());
            using var cacheSqliteContext = TestUtils.CreateSqliteContext();
            var thumbnailsCache = new ThumbnailsCacheDatabaseStorage(cacheSqliteContext);

            var thumbnailsService = new ThumbnailsService(
                fileService,
                new MimeTypeService(MimeTypeRules.TestRules),
                thumbnailsCache);
            var imageFileRenderer = new ImageFileRenderer(fileService);
            var testFileRenderer = new TestImageFileRenderer(imageFileRenderer);
            thumbnailsService.RegisterRenderer(testFileRenderer);

            await fileService.WriteFile(
                Url.Parse("file://test/Test Image.png"),
                Resources.ReadEmbeddedFile(typeof(ThumbnailsServiceTests).Assembly, "/Resources/Test Image.png"));

            var thumbnail = await thumbnailsService.GetThumbnail(
                Url.Parse("file://test/Test Image.png"),
                new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await TestUtils.SaveResult("256w.png", thumbnail.GetStream());
            Assert.AreEqual(1, testFileRenderer.RenderCount);
            Assert.AreEqual(1, await thumbnailsCache.GetCount());

            await fileService.WriteFile(
                Url.Parse("file://test/Test Image.png"),
                Resources.ReadEmbeddedFile(typeof(ThumbnailsServiceTests).Assembly, "/Resources/Transparent.png"));

            thumbnail = await thumbnailsService.GetThumbnail(
                Url.Parse("file://test/Test Image.png"),
                new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await TestUtils.SaveResult("256w.png", thumbnail.GetStream());
            Assert.AreEqual(2, testFileRenderer.RenderCount);

            await fileService.WaitComplete();

            Assert.AreEqual(1, await thumbnailsCache.GetCount());
            await fileService.Delete(Url.Parse("file://test/Test Image.png"), false);
            await fileService.WaitComplete();

            Assert.AreEqual(0, await thumbnailsCache.GetCount());
        }

        private class TestImageFileRenderer : IThumbnailsRenderer
        {
            private readonly IThumbnailsRenderer _wrappedThumbnailsRenderer;

            public TestImageFileRenderer(IThumbnailsRenderer wrappedThumbnailsRenderer)
            {
                _wrappedThumbnailsRenderer = wrappedThumbnailsRenderer;
            }

            public int RenderCount { get; private set; }

            public Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderFileInfo fileInfo, ThumbnailsRenderOption option)
            {
                RenderCount++;
                return _wrappedThumbnailsRenderer.Render(ctx, fileInfo, option);
            }

            public bool IsSupported(ThumbnailsRenderFileInfo fileInfo)
            {
                return _wrappedThumbnailsRenderer.IsSupported(fileInfo);
            }
        }
    }
}
