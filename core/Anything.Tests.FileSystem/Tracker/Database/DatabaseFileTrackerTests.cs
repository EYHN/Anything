using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem.Tracker.Database
{
    public class DatabaseFileTrackerTests
    {
        [Test]
        public async Task FeatureTest()
        {
            using var tracker = new DatabaseHintFileTracker();

            await TestHintTracker(tracker);
        }

        [Test]
        public async Task EnumerateAllFilesTest()
        {
            using var tracker = new DatabaseHintFileTracker();

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "1", FileType.Directory)));
            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("1", "1", FileType.File)) }
                        .ToImmutableArray()));

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/g/h"), new FileRecord("1", "1", FileType.File)));

            await tracker.WaitComplete();

            var expected = new[]
            {
                Url.Parse("file:///a/b/c"), Url.Parse("file:///a/b/c/e"), Url.Parse("file:///a/b/c/f"), Url.Parse("file:///a/g/h")
            };
            Assert.False(expected.Except(await tracker.EnumerateAllFiles(Url.Parse("file:///a")).ToArrayAsync()).Any());

            expected = new[] { Url.Parse("file:///a/g/h") };

            Assert.False(expected.Except(await tracker.EnumerateAllFiles(Url.Parse("file:///a/g")).ToArrayAsync()).Any());

            Assert.DoesNotThrowAsync(async () => await tracker.EnumerateAllFiles(Url.Parse("file:///a/foo")).ToArrayAsync());
        }

        private static async Task TestHintTracker(IHintFileTracker tracker)
        {
            var fileEventsHandler = new FileEventsHandler();

            tracker.FileEvent.On(fileEventsHandler.HandleFileEvents);

            void AssertWithEvent(FileEvent[] expectedEvents)
            {
                tracker.WaitComplete().AsTask().Wait();
                fileEventsHandler.AssertWithEvent(expectedEvents);
            }

            // index file test
            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "1", FileType.Directory)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b"), new FileRecord("1", "1", FileType.Directory)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b")) });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "1", FileType.File)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "2", FileType.Directory)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("1", "2", FileType.File)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c/d")) });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d"))
                });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("2", "2", FileType.Directory)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c/d"), new FileRecord("2", "2", FileType.File)));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d")) });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("3", "2", FileType.File)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c/e"), new FileRecord("1", "1", FileType.Directory)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await tracker.CommitHint(new DeletedHint(Url.Parse("file:///a/b/c")));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e")) });

            // index directory test
            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("1", "1", FileType.File)) }
                        .ToImmutableArray()));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/e"))
                });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///abc"),
                    new[] { ("h", new FileRecord("1", "1", FileType.Directory)), ("i", new FileRecord("1", "1", FileType.File)) }
                        .ToImmutableArray()));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///abc/h")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///abc/i"))
                });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c/f"),
                    new[] { ("j", new FileRecord("1", "1", FileType.Directory)), ("k", new FileRecord("1", "1", FileType.File)) }
                        .ToImmutableArray()));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f/j")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f/k"))
                });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("2", "1", FileType.Directory)) }
                        .ToImmutableArray()));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f")) });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("3", "1", FileType.File)) }
                        .ToImmutableArray()));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/j")),
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/k")),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f"))
                });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b/c"),
                    new[] { ("e", new FileRecord("1", "2", FileType.Directory)) }.ToImmutableArray()));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f")),
                    new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c/e"))
                });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("4", "2", FileType.Directory)) }.ToImmutableArray()));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c")) });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("4", "3", FileType.Directory)) }.ToImmutableArray()));
            AssertWithEvent(new[] { new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c")) });

            await tracker.CommitHint(
                new DirectoryHint(
                    Url.Parse("file:///a/b"),
                    new[] { ("c", new FileRecord("5", "3", FileType.Directory)) }.ToImmutableArray()));
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
            await tracker.CommitHint(
                new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory)));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(
                        FileEvent.EventType.Changed,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileAttachedData("metadata1", FileAttachedData.DeletionPolicies.WhenFileContentChanged) }
                            .ToImmutableArray())
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
                        new[] { new FileAttachedData("metadata2") }.ToImmutableArray()),
                    new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"))
                });

            await tracker.CommitHint(new DeletedHint(Url.Parse("file:///a/b/c")));
            AssertWithEvent(
                new[]
                {
                    new FileEvent(
                        FileEvent.EventType.Deleted,
                        Url.Parse("file:///a/b/c"),
                        new[] { new FileAttachedData("metadata3") }.ToImmutableArray())
                });
        }
    }
}
