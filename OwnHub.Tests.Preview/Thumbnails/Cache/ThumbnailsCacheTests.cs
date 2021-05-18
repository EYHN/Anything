using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Preview.Thumbnails;
using OwnHub.Preview.Thumbnails.Cache;
using OwnHub.Utils;

namespace OwnHub.Tests.Preview.Thumbnails.Cache
{
    public class ThumbnailsCacheTests
    {
        public class TestThumbnail : IThumbnail
        {
            public string ImageFormat { get; }

            public int Size { get; }

            private readonly byte[] _data;

            public TestThumbnail(byte[] data, string imageType, int size)
            {
                Size = size;
                ImageFormat = imageType;
                _data = data;
            }

            public Stream GetStream()
            {
                return new MemoryStream(_data);
            }
        }

        [Test]
        public async Task FeatureTests()
        {
            var sqliteContext = TestUtils.CreateSqliteContext("test");
            var iconsCacheStorage = new ThumbnailsCacheDatabaseStorage(sqliteContext);
            await iconsCacheStorage.Create();

            byte[] ReadStream(Stream stream)
            {
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }

            void AssertThumbnail(IThumbnail expected, IThumbnail actual)
            {
                Assert.AreEqual(expected.ImageFormat, actual.ImageFormat);
                Assert.AreEqual(expected.Size, actual.Size);
                Assert.AreEqual(ReadStream(expected.GetStream()), ReadStream(actual.GetStream()));
            }

            // cache test
            {
                var thumbnail = new TestThumbnail(Convert.FromHexString("313233"), "image/png", 100);
                await iconsCacheStorage.Cache(
                    Url.Parse("file:///a/b/c.jpg"),
                    "1",
                    thumbnail);
                var caches = await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), "1");
                Assert.AreEqual(1, caches.Length);
                AssertThumbnail(thumbnail, caches[0]);
            }

            // modify cache test
            {
                var thumbnail = new TestThumbnail(Convert.FromHexString("010203"), "image/png", 100);
                await iconsCacheStorage.Cache(
                    Url.Parse("file:///a/b/c.jpg"),
                    "1",
                    thumbnail);
                var caches = await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), "1");
                Assert.AreEqual(1, caches.Length);
                AssertThumbnail(thumbnail, caches[0]);
            }

            // cache with different size test
            {
                var thumbnail = new TestThumbnail(Convert.FromHexString("515253"), "image/png", 256);
                await iconsCacheStorage.Cache(Url.Parse("file:///a/b/c.jpg"), "1", thumbnail);
                var caches = await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), "1");
                Assert.AreEqual(2, caches.Length);
                AssertThumbnail(thumbnail, caches.First(c => c.Size == 256));
            }

            // get cache with different tag
            Assert.AreEqual(
                0,
                (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), "2")).Length);

            // cache with different url
            {
                var thumbnail1 = new TestThumbnail(Convert.FromHexString("616263"), "image/png", 256);
                var thumbnail2 = new TestThumbnail(Convert.FromHexString("717273"), "image/png", 256);
                await iconsCacheStorage.Cache(Url.Parse("file:///a/b/d.jpg"), "1", thumbnail1);
                await iconsCacheStorage.Cache(Url.Parse("file:///a/b/e.jpg"), "1", thumbnail2);
                AssertThumbnail(
                    thumbnail1,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/d.jpg"), "1"))[0]);
                AssertThumbnail(
                    thumbnail2,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/e.jpg"), "1"))[0]);
            }

            // delete file
            {
                await iconsCacheStorage.Delete(Url.Parse("file:///a/b/c.jpg"));
                Assert.AreEqual(
                    0,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), "1")).Length);
                Assert.AreEqual(
                    0,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), "1")).Length);
            }

            // delete batch
            await iconsCacheStorage.DeleteBatch(new[] { Url.Parse("file:///a/b/d.jpg"), Url.Parse("file:///a/b/e.jpg") });
            Assert.AreEqual(
                0,
                (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/d.jpg"), "1")).Length);
            Assert.AreEqual(
                0,
                (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/e.jpg"), "1")).Length);
        }
    }
}
