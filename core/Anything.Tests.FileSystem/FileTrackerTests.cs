using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class FileTrackerTests
    {
        [Test]
        public async Task DatabaseFileTrackerTests()
        {
            var context = TestUtils.CreateSqliteContext();
            var tracker = new DatabaseFileTracker(context);
            await tracker.Create();

            await TestFileTracker(tracker);
        }

        private async Task TestFileTracker(IFileTracker tracker)
        {
            List<FileChangeEvent> eventsCache = new();

            tracker.OnFileChange += events =>
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
                                expected.Tags.Select(r => r.Key + ":" + r.Data).OrderBy(t => t).ToArray();
                            return eventsCache.Any(
                                e =>
                                {
                                    var trackers =
                                        e.Tags.Select(r => r.Key + ":" + r.Data).OrderBy(t => t).ToArray();

                                    return e.Url == expected.Url && e.Type == expected.Type &&
                                           string.Join(',', trackers) == string.Join(
                                               ',',
                                               expectedMetadata);
                                });
                        }));
                eventsCache.Clear();
            }

            // index file test
            await tracker.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await tracker.IndexFile(Url.Parse("file:///a/b"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b")) });

            await tracker.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "1", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await tracker.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await tracker.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c/d")) });

            await tracker.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d"))
                });

            await tracker.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("2", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await tracker.IndexFile(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await tracker.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("3", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await tracker.IndexFile(Url.Parse("file:///a/b/c/e"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await tracker.IndexFile(Url.Parse("file:///a/b/c"), null);
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")) });

            // index directory test
            await tracker.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now))
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await tracker.IndexDirectory(
                Url.Parse("file:///abc"),
                new[]
                {
                    ("h", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("i", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now))
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///abc/h")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///abc/i"))
                });

            await tracker.IndexDirectory(
                Url.Parse("file:///a/b/c/f"),
                new[]
                {
                    ("j", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("k", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now))
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f/j")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f/k"))
                });

            await tracker.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("2", "1", FileType.Directory, DateTimeOffset.Now))
                });
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f")) });

            await tracker.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("3", "1", FileType.File, DateTimeOffset.Now))
                });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/j")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/k")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f"))
                });

            await tracker.IndexDirectory(
                Url.Parse("file:///a/b/c"),
                new[] { ("e", new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now)) });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c/e"))
                });

            await tracker.IndexDirectory(
                Url.Parse("file:///a/b"),
                new[] { ("c", new FileRecord("4", "2", FileType.Directory, DateTimeOffset.Now)) });
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await tracker.IndexDirectory(
                Url.Parse("file:///a/b"),
                new[] { ("c", new FileRecord("4", "3", FileType.Directory, DateTimeOffset.Now)) });
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await tracker.IndexDirectory(
                Url.Parse("file:///a/b"),
                new[] { ("c", new FileRecord("5", "3", FileType.Directory, DateTimeOffset.Now)) });
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            // metadata test
            await tracker.AttachTag(Url.Parse("file:///a/b/c"), new FileTrackTag("metadata1", "hello"));
            await tracker.IndexFile(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(
                        FileChangeEvent.EventType.Changed,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileTrackTag("metadata1", "hello") })
                });

            var metadata = await tracker.GetTags(Url.Parse("file:///a/b/c"));
            Assert.AreEqual(metadata[0].Key, "metadata1");
            Assert.AreEqual(metadata[0].Data, "hello");

            Assert.CatchAsync(async () => await tracker.AttachTag(Url.Parse("file:///a/b/c"), new FileTrackTag("metadata1", "hello")));
            Assert.DoesNotThrowAsync(
                async () => await tracker.AttachTag(
                    Url.Parse("file:///a/b/c"),
                    new FileTrackTag("metadata1", "hello world"),
                    true));
            await tracker.AttachTag(Url.Parse("file:///a/b/c"), new FileTrackTag("metadata2", "hello world"));

            await tracker.IndexFile(Url.Parse("file:///a/b/c"), null);
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(
                        FileChangeEvent.EventType.Deleted,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileTrackTag("metadata1", "hello world"), new FileTrackTag("metadata2", "hello world") })
                });

            Assert.CatchAsync(
                async () => await tracker.AttachTag(Url.Parse("file:///a/b/c/e"), new FileTrackTag("metadata1", "hello")));
        }
    }
}
