using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using StagingBox.FileSystem;
using StagingBox.FileSystem.Provider;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails;
using StagingBox.Preview.Thumbnails.Cache;
using StagingBox.Preview.Thumbnails.Renderers;
using StagingBox.Utils;

namespace StagingBox.Tests.Preview.Thumbnails
{
    public class ThumbnailsServiceTests
    {
        private class TestImageFileRenderer : IThumbnailsRenderer
        {
            public int RenderCount { get; private set; } = 0;

            private readonly IThumbnailsRenderer _wrappedThumbnailsRenderer;

            public TestImageFileRenderer(IThumbnailsRenderer wrappedThumbnailsRenderer)
            {
                _wrappedThumbnailsRenderer = wrappedThumbnailsRenderer;
            }

            public Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderOption option)
            {
                RenderCount++;
                return _wrappedThumbnailsRenderer.Render(ctx, option);
            }

            public bool IsSupported(ThumbnailsRenderOption option)
            {
                return _wrappedThumbnailsRenderer.IsSupported(option);
            }
        }

        [Test]
        public async Task FeatureTest()
        {
            var fileSystem = new VirtualFileSystemService();
            fileSystem.RegisterFileSystemProvider(
                "test",
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(ThumbnailsServiceTests).Assembly)));
            var sqliteContext = TestUtils.CreateSqliteContext("test");
            var thumbnailsCacheDatabaseStorage = new ThumbnailsCacheDatabaseStorage(sqliteContext);
            await thumbnailsCacheDatabaseStorage.Create();

            var thumbnailsService = new ThumbnailsService(
                fileSystem,
                new MimeTypeService(MimeTypeRules.TestRules),
                new ThumbnailsCacheDatabaseStorage(sqliteContext));
            var imageFileRenderer = new ImageFileRenderer(fileSystem);
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
    }
}
