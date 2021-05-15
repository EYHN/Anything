﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.FileSystem;
using OwnHub.Preview;
using OwnHub.Utils;

namespace OwnHub.Tests.Preview
{
    public class IconsCacheTests
    {
        [Test]
        public async Task FeatureTests()
        {
            var sqliteContext = TestUtils.CreateSqliteContext();
            var iconsCacheStorage = new IconsCacheDatabaseStorage(sqliteContext);
            await iconsCacheStorage.Create();
            var fileRecord = new FileRecord("a", "c", FileType.File, DateTimeOffset.Now);

            // cache test
            await iconsCacheStorage.Cache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "100x100", Convert.FromHexString("313233"));
            Assert.AreEqual(
                Convert.FromHexString("313233"),
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "100x100"));

            // modify cache test
            await iconsCacheStorage.Cache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "100x100", Convert.FromHexString("010203"));
            Assert.AreEqual(
                Convert.FromHexString("010203"),
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "100x100"));

            // cache with different key test
            await iconsCacheStorage.Cache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "256x256", Convert.FromHexString("515253"));
            Assert.AreEqual(
                Convert.FromHexString("515253"),
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "256x256"));

            // get cache with different file record
            Assert.AreEqual(
                null,
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord with { ContentTag = "b" }, "256x256"));

            // cache with different url
            await iconsCacheStorage.Cache(Url.Parse("file:///a/b/d.jpg"), fileRecord, "256x256", Convert.FromHexString("616263"));
            await iconsCacheStorage.Cache(Url.Parse("file:///a/b/e.jpg"), fileRecord, "256x256", Convert.FromHexString("717273"));
            Assert.AreEqual(
                Convert.FromHexString("616263"),
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/d.jpg"), fileRecord, "256x256"));
            Assert.AreEqual(
                Convert.FromHexString("717273"),
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/e.jpg"), fileRecord, "256x256"));

            // delete file
            await iconsCacheStorage.Delete(Url.Parse("file:///a/b/c.jpg"));
            Assert.AreEqual(
                null,
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "100x100"));
            Assert.AreEqual(
                null,
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/c.jpg"), fileRecord, "256x256"));

            // delete batch
            await iconsCacheStorage.DeleteBatch(new[] { Url.Parse("file:///a/b/d.jpg"), Url.Parse("file:///a/b/e.jpg") });
            Assert.AreEqual(
                null,
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/d.jpg"), fileRecord, "256x256"));
            Assert.AreEqual(
                null,
                await iconsCacheStorage.GetCache(Url.Parse("file:///a/b/e.jpg"), fileRecord, "256x256"));
        }
    }
}
