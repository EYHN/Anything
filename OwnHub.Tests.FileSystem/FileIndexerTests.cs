using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.FileSystem;
using OwnHub.FileSystem.Indexer;
using OwnHub.FileSystem.Indexer.Database;
using OwnHub.Utils;

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
            List<FileChangeEvent> eventsCache = new();

            indexer.OnFileChange += events =>
            {
                foreach (var @event in events)
                {
                    eventsCache.Add(@event);
                }
            };


            void AssertWithEvent(FileChangeEvent[] expectedEvents)
            {
                Assert.IsTrue(
                    expectedEvents.Length == eventsCache.Count && expectedEvents.All(
                        expected =>
                        {
                            var expectedMetadata =
                                expected.Metadata.Select(r => r.Key + ":" + r.Data).OrderBy(t => t).ToArray();
                            return eventsCache.Any(
                                (e) =>
                                {
                                    var trackers =
                                        e.Metadata.Select(r => r.Key + ":" + r.Data).OrderBy(t => t).ToArray();

                                    return e.Url == expected.Url && e.Type == expected.Type &&
                                           string.Join(',', trackers) == string.Join(
                                               ',',
                                               expectedMetadata);
                                });
                        }));
                eventsCache.Clear();
            }

            // index file test
            await indexer.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await indexer.IndexFile(Url.Parse("file:///a/b"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b")) });

            await indexer.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "1", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await indexer.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await indexer.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c/d")) });

            await indexer.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d"))
                });

            await indexer.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("2", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await indexer.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await indexer.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("3", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await indexer.IndexFile(Url.Parse("file:///a/b/c/e"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await indexer.IndexFile(Url.Parse("file:///a/b/c"), null);
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")) });

            // index directory test
            await indexer.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await indexer.IndexDirectory(
                Url.Parse("file:///abc"),
                new[]
                {
                    ("h", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("i", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///abc/h")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///abc/i"))
                });

            await indexer.IndexDirectory(
                Url.Parse("file:///a/b/c/f"),
                new[]
                {
                    ("j", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("k", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f/j")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f/k"))
                });

            await indexer.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("2", "1", FileType.Directory, DateTimeOffset.Now)),
                });
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f")) });

            await indexer.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("3", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/j")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/k")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f"))
                });

            await indexer.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[] { ("e", new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c/e"))
                });

            await indexer.IndexDirectory(
                Url.Parse("file:///a/b"),
                new[] { ("c", new FileRecord("4", "2", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await indexer.IndexDirectory(
                Url.Parse("file:///a/b"),
                new[] { ("c", new FileRecord("4", "3", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await indexer.IndexDirectory(
                Url.Parse("file:///a/b"),
                new[] { ("c", new FileRecord("5", "3", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            // metadata test
            await indexer.AttachMetadata(Url.Parse("file:///a/b/c"), new FileMetadata("metadata1", "hello"));
            await indexer.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(
                        FileChangeEvent.EventType.Changed,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileMetadata("metadata1", "hello") })
                });

            var metadata = await indexer.GetMetadata(Url.Parse("file:///a/b/c"));
            Assert.AreEqual(metadata[0].Key, "metadata1");
            Assert.AreEqual(metadata[0].Data, "hello");

            Assert.CatchAsync(async () => await indexer.AttachMetadata(Url.Parse("file:///a/b/c"), new FileMetadata("metadata1", "hello")));
            Assert.DoesNotThrowAsync(
                async () => await indexer.AttachMetadata(
                    Url.Parse("file:///a/b/c"),
                    new FileMetadata("metadata1", "hello world"),
                    replace: true));
            await indexer.AttachMetadata(Url.Parse("file:///a/b/c"), new FileMetadata("metadata2", "hello world"));

            await indexer.IndexFile(Url.Parse("file:///a/b/c"), null);
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(
                        FileChangeEvent.EventType.Deleted,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileMetadata("metadata1", "hello world"), new FileMetadata("metadata2", "hello world") })
                });

            Assert.CatchAsync(
                async () => await indexer.AttachMetadata(Url.Parse("file:///a/b/c/e"), new FileMetadata("metadata1", "hello")));
        }
    }
}
