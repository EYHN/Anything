using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.FileSystem;
using OwnHub.FileSystem.Indexer;
using OwnHub.FileSystem.Indexer.Database;

namespace OwnHub.Tests.FileSystem
{
    public class FileIndexerTests
    {
        [Test]
        public async Task DatabaseFileIndexerTests()
        {
            var context = TestUtils.CreateSqliteContext();
            var indexer = new DatabaseFileIndexer(context, "File");
            await indexer.Create();

            await TestFileIndexer(indexer);
        }

        private async Task TestFileIndexer(IFileIndexer indexer)
        {
            List<IFileIndexer.ChangeEvent> eventsCache = new();

            indexer.OnFileChange += events =>
            {
                foreach (var @event in events)
                {
                    eventsCache.Add(@event);
                }
            };


            void AssertWithEvent(IFileIndexer.ChangeEvent[] expectedEvents)
            {
                Assert.IsTrue(
                    expectedEvents.Length == eventsCache.Count && expectedEvents.All(
                        expected =>
                        {
                            var expectedMetadata =
                                expected.Metadata?.Select(r => r.Key + ":" + r.Data).OrderBy(t => t).ToArray();
                            return eventsCache.Any(
                                (e) =>
                                {
                                    var trackers =
                                        e.Metadata?.Select(r => r.Key + ":" + r.Data).OrderBy(t => t).ToArray();

                                    return e.Path == expected.Path && e.Type == expected.Type &&
                                           string.Join(',', trackers ?? Array.Empty<string>()) == string.Join(
                                               ',',
                                               expectedMetadata ?? Array.Empty<string>());
                                });
                        }));
                eventsCache.Clear();
            }

            // index file test
            await indexer.IndexFile("/a/b/c", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c") });

            await indexer.IndexFile("/a/b", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b") });

            await indexer.IndexFile("/a/b/c/d", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/d") });

            await indexer.IndexFile("/a/b/c", new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Changed, "/a/b/c") });

            await indexer.IndexFile("/a/b/c/d", new FileRecord("1", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Changed, "/a/b/c/d") });

            await indexer.IndexFile("/a/b/c/d", new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/d"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/d")
                });

            await indexer.IndexFile("/a/b/c", new FileRecord("2", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/d"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c")
                });

            await indexer.IndexFile("/a/b/c/d", new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/d") });

            await indexer.IndexFile("/a/b/c", new FileRecord("3", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/d"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c")
                });

            await indexer.IndexFile("/a/b/c/e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/e")
                });

            await indexer.IndexFile("/a/b/c", null);
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/e") });

            // index directory test
            await indexer.IndexDirectory(
                "/a/b/c",
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/f"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/e")
                });

            await indexer.IndexDirectory(
                "/abc",
                new[]
                {
                    ("h", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("i", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/abc/h"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/abc/i")
                });

            await indexer.IndexDirectory(
                "/a/b/c/f",
                new[]
                {
                    ("j", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("k", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/f"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/f/j"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/f/k")
                });

            await indexer.IndexDirectory(
                "/a/b/c",
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("2", "1", FileType.Directory, DateTimeOffset.Now)),
                });
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/f") });

            await indexer.IndexDirectory(
                "/a/b/c",
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("3", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/f"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/f/j"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/f/k"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c/f")
                });

            await indexer.IndexDirectory(
                "/a/b/c",
                new[] { ("e", new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/f"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Changed, "/a/b/c/e")
                });

            await indexer.IndexDirectory(
                "/a/b",
                new[] { ("c", new FileRecord("4", "2", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c") });

            await indexer.IndexDirectory(
                "/a/b",
                new[] { ("c", new FileRecord("4", "3", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(new[] { new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Changed, "/a/b/c") });

            await indexer.IndexDirectory(
                "/a/b",
                new[] { ("c", new FileRecord("5", "3", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Deleted, "/a/b/c/e"),
                    new IFileIndexer.ChangeEvent(IFileIndexer.EventType.Created, "/a/b/c")
                });

            // metadata test
            await indexer.AttachMetadata("/a/b/c", new IFileIndexer.Metadata("metadata1", "hello"));
            await indexer.IndexFile("/a/b/c", new FileRecord("5", "4", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(
                        IFileIndexer.EventType.Changed,
                        "/a/b/c",
                        new[] { new IFileIndexer.Metadata("metadata1", "hello") })
                });

            Assert.CatchAsync(async () => await indexer.AttachMetadata("/a/b/c", new IFileIndexer.Metadata("metadata1", "hello")));
            Assert.DoesNotThrowAsync(
                async () => await indexer.AttachMetadata("/a/b/c", new IFileIndexer.Metadata("metadata1", "hello world"), replace: true));
            await indexer.AttachMetadata("/a/b/c", new IFileIndexer.Metadata("metadata2", "hello world"));

            await indexer.IndexFile("/a/b/c", null);
            AssertWithEvent(
                new[]
                {
                    new IFileIndexer.ChangeEvent(
                        IFileIndexer.EventType.Deleted,
                        "/a/b/c",
                        new[] { new IFileIndexer.Metadata("metadata1", "hello world"), new IFileIndexer.Metadata("metadata2", "hello world") })
                });

            Assert.CatchAsync(async () => await indexer.AttachMetadata("/a/b/c/e", new IFileIndexer.Metadata("metadata1", "hello")));
        }
    }
}
