using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Tracker.Database;
using Anything.FileSystem.Tracker.Memory;
using Anything.Utils;
using Anything.Utils.Event;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class FileTrackerTests
    {
        [Test]
        public async Task DatabaseFileTrackerTests()
        {
            var context = TestUtils.CreateSqliteContext();
            var mockedFileHintProvider = new MockedFileHintProvider();
            var tracker = new DatabaseFileTracker(mockedFileHintProvider, context);
            await tracker.Create();

            await TestFileTracker(tracker, mockedFileHintProvider);
        }

        [Test]
        public async Task MemoryFileTrackerTests()
        {
            var mockedFileHintProvider = new MockedFileHintProvider();
            var tracker = new MemoryFileTracker(mockedFileHintProvider);
            await tracker.Create();

            await TestFileTracker(tracker, mockedFileHintProvider);
        }

        private async Task TestFileTracker(IFileTracker tracker, MockedFileHintProvider mockedFileHintProvider)
        {
            List<FileChangeEvent> eventsCache = new();

            tracker.OnFileChange.On(events =>
            {
                foreach (var @event in events)
                {
                    eventsCache.Add(@event);
                }
            });

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
            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b")) });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now)));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "2", FileType.File, DateTimeOffset.Now)));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c/d")) });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File, DateTimeOffset.Now)));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d"))
                });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("2", "2", FileType.Directory, DateTimeOffset.Now)));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File, DateTimeOffset.Now)));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("3", "2", FileType.File, DateTimeOffset.Now)));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/e"), new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await mockedFileHintProvider.DeletedHintEventEmitter.EmitAsync(new DeletedHint(Url.Parse("file:///a/b/c")));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")) });

            // index directory test
            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[]
                    {
                        ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                        ("f", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now))
                    }));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///abc"),
                    new[]
                    {
                        ("h", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                        ("i", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now))
                    }));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///abc/h")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///abc/i"))
                });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c/f"),
                    new[]
                    {
                        ("j", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                        ("k", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now))
                    }));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f/j")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f/k"))
                });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[]
                    {
                        ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                        ("f", new FileRecord("2", "1", FileType.Directory, DateTimeOffset.Now))
                    }));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f")) });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[]
                    {
                        ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                        ("f", new FileRecord("3", "1", FileType.File, DateTimeOffset.Now))
                    }));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/j")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/k")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c/f"))
                });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now)) }));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c/e"))
                });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("4", "2", FileType.Directory, DateTimeOffset.Now)) }));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("4", "3", FileType.Directory, DateTimeOffset.Now)) }));
            AssertWithEvent(new[] { new FileChangeEvent(FileChangeEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await mockedFileHintProvider.DirectoryHintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("5", "3", FileType.Directory, DateTimeOffset.Now)) }));
            AssertWithEvent(
                new[]
                {
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")),
                    new FileChangeEvent(FileChangeEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            // metadata test
            await tracker.AttachTag(Url.Parse("file:///a/b/c"), new FileTrackTag("metadata1", "hello"));
            await mockedFileHintProvider.FileHintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory, DateTimeOffset.Now)));
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

            await mockedFileHintProvider.DeletedHintEventEmitter.EmitAsync(new DeletedHint(Url.Parse("file:///a/b/c")));
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

        private class MockedFileHintProvider : IFileHintProvider
        {
            public EventEmitter<FileHint> FileHintEventEmitter { get; } = new();

            public EventEmitter<DirectoryHint> DirectoryHintEventEmitter { get; } = new();

            public EventEmitter<DeletedHint> DeletedHintEventEmitter { get; } = new();

            public Event<FileHint> OnFileHint => FileHintEventEmitter.Event;

            public Event<DirectoryHint> OnDirectoryHint => DirectoryHintEventEmitter.Event;

            public Event<DeletedHint> OnDeletedHint => DeletedHintEventEmitter.Event;
        }
    }
}
