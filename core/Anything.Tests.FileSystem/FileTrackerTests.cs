using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Tracker.Database;
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
            var mockedFileHintProvider = new MockedHintProvider();
            var tracker = new DatabaseFileTracker(mockedFileHintProvider, context);
            await tracker.Create();

            await TestFileTracker(tracker, mockedFileHintProvider);
        }

        private async Task TestFileTracker(IFileTracker tracker, MockedHintProvider mockedHintProvider)
        {
            List<FileEvent> eventsCache = new();

            tracker.FileEvent.On(events =>
            {
                foreach (var @event in events)
                {
                    eventsCache.Add(@event);
                }
            });

            void AssertWithEvent(FileEvent[] expectedEvents)
            {
                tracker.WaitComplete().AsTask().Wait();
                Assert.IsTrue(
                    expectedEvents.Length == eventsCache.Count && expectedEvents.All(
                        expected =>
                        {
                            var expectedMetadata =
                                expected.AttachedData.Select(r => r.Payload + ':' + r.DeletionPolicy).OrderBy(t => t).ToArray();
                            return eventsCache.Any(
                                e =>
                                {
                                    var trackers =
                                        e.AttachedData.Select(r => r.Payload + ':' + r.DeletionPolicy).OrderBy(t => t).ToArray();

                                    return e.Url == expected.Url && e.Type == expected.Type &&
                                           string.Join(',', trackers) == string.Join(
                                               ',',
                                               expectedMetadata);
                                });
                        }));
                eventsCache.Clear();
            }

            // index file test
            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "1", FileType.Directory)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b"), new FileRecord("1", "1", FileType.Directory)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "1", FileType.File)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "2", FileType.Directory)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "2", FileType.File)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c/d")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("2", "2", FileType.Directory)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("3", "2", FileType.File)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c/e"), new FileRecord("1", "1", FileType.Directory)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(new DeletedHint(Url.Parse("file:///a/b/c")));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")) });

            // index directory test
            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("1", "1", FileType.File)) }));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///abc"),
                    new[] { ("h", new FileRecord("1", "1", FileType.Directory)), ("i", new FileRecord("1", "1", FileType.File)) }));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///abc/h")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///abc/i"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c/f"),
                    new[] { ("j", new FileRecord("1", "1", FileType.Directory)), ("k", new FileRecord("1", "1", FileType.File)) }));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f/j")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f/k"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("2", "1", FileType.Directory)) }));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("3", "1", FileType.File)) }));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/j")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/k")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "2", FileType.Directory)) }));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c/e"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("4", "2", FileType.Directory)) }));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("4", "3", FileType.Directory)) }));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("5", "3", FileType.Directory)) }));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            // attach data test
            await tracker.AttachData(
                Url.Parse("file:///a/b/c"),
                new FileRecord("5", "3", FileType.Directory),
                new FileAttachedData("metadata1", FileAttachedData.DeletionPolicies.WhenFileContentChanged));
            await mockedHintProvider.HintEventEmitter.EmitAsync(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(
                        FileEvent.EventType.Changed,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileAttachedData("metadata1", FileAttachedData.DeletionPolicies.WhenFileContentChanged) })
                });

            await tracker.AttachData(
                Url.Parse("file:///a/b/c"),
                new FileRecord("5", "4", FileType.Directory),
                new FileAttachedData("metadata2"));

            await tracker.AttachData(
                Url.Parse("file:///a/b/c"),
                new FileRecord("6", "4", FileType.Directory),
                new FileAttachedData("metadata3"));

            AssertWithEvent(
                new[]
                {
                    new FileEvent(
                        FileEvent.EventType.Deleted,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileAttachedData("metadata2") }),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await mockedHintProvider.HintEventEmitter.EmitAsync(new DeletedHint(Url.Parse("file:///a/b/c")));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(
                        FileEvent.EventType.Deleted,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileAttachedData("metadata3") })
                });
        }

        private class MockedHintProvider : IHintProvider
        {
            public EventEmitter<Hint> HintEventEmitter { get; } = new();

            public Event<Hint> OnHint => HintEventEmitter.Event;
        }
    }
}
