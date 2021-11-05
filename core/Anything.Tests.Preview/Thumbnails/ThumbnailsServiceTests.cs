using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Preview.Mime;
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
            using var fileService = new FileService(TestUtils.Logger);
            fileService.AddFileSystem(
                "test",
                new EmbeddedFileSystem(new EmbeddedFileProvider(typeof(ThumbnailsServiceTests).Assembly)));
            using var sqliteContext = TestUtils.CreateSqliteContext();

            using var thumbnailsCacheDatabaseStorage = new ThumbnailsCacheDatabaseStorage(fileService, TestUtils.Logger);
            var thumbnailsService = new ThumbnailsService(
                fileService,
                new MimeTypeService(fileService, MimeTypeRules.TestRules),
                thumbnailsCacheDatabaseStorage);
            var imageFileRenderer = new ImageFileRenderer(fileService);
            var testFileRenderer = new TestImageFileRenderer(imageFileRenderer);
            thumbnailsService.RegisterRenderer(testFileRenderer);

            var thumbnail = await thumbnailsService.GetThumbnail(
                await fileService.CreateFileHandle(Url.Parse("file://test/Resources/Test Image.png")),
                new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await using (var stream = thumbnail.GetStream())
            {
                await TestUtils.SaveResult("256w.png", stream);
            }

            thumbnail = await thumbnailsService.GetThumbnail(
                await fileService.CreateFileHandle(Url.Parse("file://test/Resources/Test Image.png")),
                new ThumbnailOption { Size = 128, ImageFormat = "image/png" });
            Assert.AreEqual(128, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await using (var stream = thumbnail.GetStream())
            {
                await TestUtils.SaveResult("128w.png", stream);
            }

            thumbnail = await thumbnailsService.GetThumbnail(
                await fileService.CreateFileHandle(Url.Parse("file://test/Resources/Test Image.png")),
                new ThumbnailOption { Size = 512, ImageFormat = "image/png" });
            Assert.AreEqual(512, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await using (var stream = thumbnail.GetStream())
            {
                await TestUtils.SaveResult("512w.png", stream);
            }
        }

        [Test]
        public async Task CacheTest()
        {
            using var fileService = new FileService(TestUtils.Logger);
            fileService.AddFileSystem(
                "test",
                new MemoryFileSystem());
            using var cacheSqliteContext = TestUtils.CreateSqliteContext();
            using var thumbnailsCache = new ThumbnailsCacheDatabaseStorage(fileService, TestUtils.Logger);

            var thumbnailsService = new ThumbnailsService(
                fileService,
                new MimeTypeService(fileService, MimeTypeRules.TestRules),
                thumbnailsCache);
            var imageFileRenderer = new ImageFileRenderer(fileService);
            var testFileRenderer = new TestImageFileRenderer(imageFileRenderer);
            thumbnailsService.RegisterRenderer(testFileRenderer);

            var resourcesDirectory =
                await fileService.CreateDirectory(await fileService.CreateFileHandle(Url.Parse("file://test/")), "Resources");

            var testImagePng = await fileService.CreateFile(
                resourcesDirectory,
                "Test Image.png",
                Resources.ReadEmbeddedFile(typeof(ThumbnailsServiceTests).Assembly, "/Resources/Test Image.png"));

            var thumbnail = await thumbnailsService.GetThumbnail(
                testImagePng,
                new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await using (var stream = thumbnail.GetStream())
            {
                await TestUtils.SaveResult("256w.png", stream);
            }

            Assert.AreEqual(1, testFileRenderer.RenderCount);
            Assert.AreEqual(1, await thumbnailsCache.GetCount());

            thumbnail = await thumbnailsService.GetThumbnail(
                testImagePng,
                new ThumbnailOption { Size = 128, ImageFormat = "image/png" });
            Assert.AreEqual(128, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await using (var stream = thumbnail.GetStream())
            {
                await TestUtils.SaveResult("128w.png", stream);
            }

            Assert.AreEqual(1, testFileRenderer.RenderCount);
            Assert.AreEqual(2, await thumbnailsCache.GetCount());

            await fileService.WriteFile(
                testImagePng,
                Resources.ReadEmbeddedFile(typeof(ThumbnailsServiceTests).Assembly, "/Resources/Transparent.png"));

            thumbnail = await thumbnailsService.GetThumbnail(
                testImagePng,
                new ThumbnailOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, thumbnail!.Size);
            Assert.AreEqual("image/png", thumbnail.ImageFormat);
            await using (var stream = thumbnail.GetStream())
            {
                await TestUtils.SaveResult("256w.png", stream);
            }

            Assert.AreEqual(2, testFileRenderer.RenderCount);

            Assert.AreEqual(1, await thumbnailsCache.GetCount());
            await fileService.Delete(testImagePng, resourcesDirectory, "Test Image.png", false);

            await fileService.WaitFullScan();
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
