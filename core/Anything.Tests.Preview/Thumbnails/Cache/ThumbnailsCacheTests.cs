using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Cache;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Preview.Thumbnails.Cache
{
    public class ThumbnailsCacheTests
    {
        [Test]
        public async Task FeatureTests()
        {
            using var sqliteContext = TestUtils.CreateSqliteContext("test");
            var iconsCacheStorage = new ThumbnailsCacheDatabaseStorage(sqliteContext);
            var fileRecord = new FileRecord("1", "1", FileType.File);

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
            long cCacheId1;
            {
                var thumbnail = new TestThumbnail(Convert.FromHexString("313233"), "image/png", 100);
                cCacheId1 = await iconsCacheStorage.Cache(
                    Url.Parse("file:///a/b/c.jpg"),
                    fileRecord,
                    thumbnail);
                var caches = await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord);
                Assert.AreEqual(1, caches.Length);
                AssertThumbnail(thumbnail, caches[0]);
            }

            // cache test
            long cCacheId2;
            {
                var thumbnail = new TestThumbnail(Convert.FromHexString("010203"), "image/png", 100);
                cCacheId2 = await iconsCacheStorage.Cache(
                    Url.Parse("file:///a/b/c.jpg"),
                    fileRecord,
                    thumbnail);
                var caches = await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord);
                Assert.AreEqual(2, caches.Length);
            }

            // cache with different size test
            long cCacheId3;
            {
                var thumbnail = new TestThumbnail(Convert.FromHexString("515253"), "image/png", 256);
                cCacheId3 = await iconsCacheStorage.Cache(Url.Parse("file:///a/b/c.jpg"), fileRecord, thumbnail);
                var caches = await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord);
                Assert.AreEqual(3, caches.Length);
                AssertThumbnail(thumbnail, caches.First(c => c.Size == 256));
            }

            // cache with different url
            long dCacheId;
            long eCacheId;
            {
                var thumbnail1 = new TestThumbnail(Convert.FromHexString("616263"), "image/png", 256);
                var thumbnail2 = new TestThumbnail(Convert.FromHexString("717273"), "image/png", 256);
                dCacheId = await iconsCacheStorage.Cache(Url.Parse("file:///a/b/d.jpg"), fileRecord, thumbnail1);
                eCacheId = await iconsCacheStorage.Cache(Url.Parse("file:///a/b/e.jpg"), fileRecord, thumbnail2);
                AssertThumbnail(
                    thumbnail1,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/d.jpg"), fileRecord))[0]);
                AssertThumbnail(
                    thumbnail2,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/e.jpg"), fileRecord))[0]);
            }

            // delete file
            {
                await iconsCacheStorage.Delete(dCacheId);
                Assert.AreEqual(
                    0,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/d.jpg"), fileRecord)).Length);
                await iconsCacheStorage.Delete(cCacheId1);
                Assert.AreEqual(
                    2,
                    (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord)).Length);

                Assert.DoesNotThrowAsync(async () => await iconsCacheStorage.Delete(cCacheId1));
            }

            // delete batch
            await iconsCacheStorage.DeleteBatch(new[] { eCacheId, cCacheId2, cCacheId3 });
            Assert.AreEqual(
                0,
                (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/e.jpg"), fileRecord)).Length);
            Assert.AreEqual(
                0,
                (await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord)).Length);
        }

        public class TestThumbnail : IThumbnail
        {
            private readonly byte[] _data;

            public TestThumbnail(byte[] data, string imageType, int size)
            {
                Size = size;
                ImageFormat = imageType;
                _data = data;
            }

            public string ImageFormat { get; }

            public int Size { get; }

            public Stream GetStream()
            {
                return new MemoryStream(_data);
            }
        }
    }
}
