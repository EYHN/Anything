// using System.Collections.Immutable;
// using System.Linq;
// using System.Threading.Tasks;
// using Anything.Database;
// using Anything.FileSystem;
// using Anything.FileSystem.Tracker;
// using Anything.FileSystem.Tracker.Database;
// using Anything.Utils;
// using NUnit.Framework;
//
// namespace Anything.Tests.FileSystem.Tracker.Database
// {
//     public class DatabaseFileTrackerTests
//     {
//         [Test]
//         public async Task FeatureTest()
//         {
//             using var trackerCacheContext = new SqliteContext();
//             var tracker = new HintFileTracker(trackerCacheContext);
//
//             await TestHintTracker(tracker);
//         }
//
//         [Test]
//         public async Task EnumerateAllFilesTest()
//         {
//             using var trackerCacheContext = new SqliteContext();
//             var tracker = new HintFileTracker(trackerCacheContext);
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("1", "1", FileType.Directory)));
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b/c"),
//                     new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("1", "1", FileType.File)) }
//                         .ToImmutableArray()));
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/g/h"), new FileRecord("1", "1", FileType.File)));
//
//             var expected = new[]
//             {
//                 Url.Parse("file:///a/b/c"), Url.Parse("file:///a/b/c/e"), Url.Parse("file:///a/b/c/f"), Url.Parse("file:///a/g/h")
//             };
//             Assert.False(expected.Except(await tracker.EnumerateAllFiles(Url.Parse("file:///a")).ToArrayAsync()).Any());
//
//             expected = new[] { Url.Parse("file:///a/g/h") };
//
//             Assert.False(expected.Except(await tracker.EnumerateAllFiles(Url.Parse("file:///a/g")).ToArrayAsync()).Any());
//
//             Assert.DoesNotThrowAsync(async () => await tracker.EnumerateAllFiles(Url.Parse("file:///a/foo")).ToArrayAsync());
//         }
//
//         private static async Task TestHintTracker(IHintFileTracker tracker)
//         {
//             var fileEventsHandler = new FileEventsHandler();
//
//             using var fileEvent = tracker.FileEvent.On(fileEventsHandler.HandleFileEvents);
//
//             void AssertWithEvent(FileEvent[] expectedEvents)
//             {
//                 fileEventsHandler.AssertWithEvent(expectedEvents);
//             }
//
//             // index file test
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c"), new("1", "1", FileType.Directory)));
//             AssertWithEvent(new[]
//             {
//                 new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"), new("1", "1", FileType.Directory))
//             });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b"), new("1", "1", FileType.Directory)));
//             AssertWithEvent(new[]
//             {
//                 new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b"), new("1", "1", FileType.Directory))
//             });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c/d"), new("1", "1", FileType.File)));
//             AssertWithEvent(
//                 new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d"), new("1", "1", FileType.File)) });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c"), new("1", "2", FileType.Directory)));
//             AssertWithEvent(new[]
//             {
//                 new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c"), new("1", "2", FileType.Directory))
//             });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c/d"), new("1", "2", FileType.File)));
//             AssertWithEvent(
//                 new[] { new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c/d"), new("1", "2", FileType.File)) });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c/d"), new("2", "2", FileType.File)));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d"), new("1", "2", FileType.File)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d"), new("2", "2", FileType.File))
//                 });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("2", "2", FileType.Directory)));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c"), new("1", "2", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d"), new("2", "2", FileType.File)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"), new("2", "2", FileType.Directory))
//                 });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c/d"), new("2", "2", FileType.File)));
//             AssertWithEvent(
//                 new[] { new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/d"), new("2", "2", FileType.File)) });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c"), new("3", "2", FileType.File)));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c"), new("2", "2", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/d"), new("2", "2", FileType.File)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"), new("3", "2", FileType.File))
//                 });
//
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c/e"), new FileRecord("1", "1", FileType.Directory)));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c"), new("3", "2", FileType.File)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/e"), new("1", "1", FileType.Directory))
//                 });
//
//             await tracker.CommitHint(new DeletedHint(Url.Parse("file:///a/b/c")));
//             AssertWithEvent(new[]
//             {
//                 new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e"), new FileRecord("1", "1", FileType.Directory))
//             });
//
//             // index directory test
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b/c"),
//                     new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("1", "1", FileType.File)) }
//                         .ToImmutableArray()));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f"), new("1", "1", FileType.File)), new FileEvent(
//                         FileEvent.EventType.Created, Url.Parse("file:///a/b/c/e"), new("1", "1", FileType.Directory))
//                 });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///abc"),
//                     new[] { ("h", new FileRecord("1", "1", FileType.Directory)), ("i", new FileRecord("1", "1", FileType.File)) }
//                         .ToImmutableArray()));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///abc/h"), new("1", "1", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///abc/i"), new("1", "1", FileType.File))
//                 });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b/c/f"),
//                     new[] { ("j", new FileRecord("1", "1", FileType.Directory)), ("k", new FileRecord("1", "1", FileType.File)) }
//                         .ToImmutableArray()));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f"), new("1", "1", FileType.File)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f/j"), new("1", "1", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f/k"), new("1", "1", FileType.File))
//                 });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b/c"),
//                     new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("2", "1", FileType.Directory)) }
//                         .ToImmutableArray()));
//             AssertWithEvent(new[]
//             {
//                 new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f"), new("2", "1", FileType.Directory))
//             });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b/c"),
//                     new[] { ("e", new FileRecord("1", "1", FileType.Directory)), ("f", new FileRecord("3", "1", FileType.File)) }
//                         .ToImmutableArray()));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f"), new("2", "1", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/j"), new("1", "1", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f/k"), new("1", "1", FileType.File)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c/f"), new("3", "1", FileType.File))
//                 });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b/c"),
//                     new[] { ("e", new FileRecord("1", "2", FileType.Directory)) }.ToImmutableArray()));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/f"), new("3", "1", FileType.File)),
//                     new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c/e"), new("1", "2", FileType.Directory))
//                 });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b"),
//                     new[] { ("c", new FileRecord("4", "2", FileType.Directory)) }.ToImmutableArray()));
//             AssertWithEvent(new[]
//             {
//                 new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"), new("4", "2", FileType.Directory))
//             });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b"),
//                     new[] { ("c", new FileRecord("4", "3", FileType.Directory)) }.ToImmutableArray()));
//             AssertWithEvent(new[]
//             {
//                 new FileEvent(FileEvent.EventType.Changed, Url.Parse("file:///a/b/c"), new("4", "3", FileType.Directory))
//             });
//
//             await tracker.CommitHint(
//                 new DirectoryHint(
//                     Url.Parse("file:///a/b"),
//                     new[] { ("c", new FileRecord("5", "3", FileType.Directory)) }.ToImmutableArray()));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c"), new("4", "3", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Deleted, Url.Parse("file:///a/b/c/e"), new("1", "2", FileType.Directory)),
//                     new FileEvent(FileEvent.EventType.Created, Url.Parse("file:///a/b/c"), new("5", "3", FileType.Directory))
//                 });
//
//             // attach data test
//             await tracker.AttachData(
//                 Url.Parse("file:///a/b/c"),
//                 new FileRecord("5", "3", FileType.Directory),
//                 new FileAttachedData("metadata1", FileAttachedData.DeletionPolicies.WhenFileContentChanged));
//             await tracker.CommitHint(
//                 new FileHint(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory)));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(
//                         FileEvent.EventType.Changed,
//                         Url.Parse("file:///a/b/c"),
//                         new ("5", "4", FileType.Directory),
//                         new[] { new FileAttachedData("metadata1", FileAttachedData.DeletionPolicies.WhenFileContentChanged) }
//                             .ToImmutableArray())
//                 });
//
//             await tracker.AttachData(
//                 Url.Parse("file:///a/b/c"),
//                 new FileRecord("5", "4", FileType.Directory),
//                 new FileAttachedData("a-1"));
//
//             // identifier not indexed
//             await tracker.AttachData(
//                 Url.Parse("file:///a/b/c"),
//                 new FileRecord("6", "4", FileType.Directory),
//                 new FileAttachedData("b-1"));
//
//             // content tag not indexed, and deletionPolicy is 'WhenFileContentChanged'
//             await tracker.AttachData(
//                 Url.Parse("file:///a/b/c"),
//                 new FileRecord("5", "5", FileType.Directory),
//                 new FileAttachedData("c-1", FileAttachedData.DeletionPolicies.WhenFileContentChanged));
//
//             await tracker.AttachData(
//                 Url.Parse("file:///a/b/c"),
//                 new FileRecord("5", "4", FileType.Directory),
//                 new FileAttachedData("d-1", FileAttachedData.DeletionPolicies.WhenFileContentChanged));
//
//             var attachedData = await tracker.GetAttachedData(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory));
//             Assert.AreEqual(attachedData.Length, 2);
//             Assert.True(!attachedData.Except(new[]
//             {
//                 new FileAttachedData("a-1"), new FileAttachedData("d-1", FileAttachedData.DeletionPolicies.WhenFileContentChanged)
//             }).Any());
//
//             attachedData = await tracker.GetAttachedData(Url.Parse("file:///a/b/c"), new FileRecord("5", "4", FileType.Directory), "a-");
//             Assert.AreEqual(attachedData.Length, 1);
//             Assert.True(!attachedData.Except(new[] { new FileAttachedData("a-1") }).Any());
//
//             // identifier tag not indexed
//             Assert.False((await tracker.GetAttachedData(Url.Parse("file:///a/b/c"), new FileRecord("6", "4", FileType.Directory))).Any());
//
//             // content tag not indexed
//             attachedData = await tracker.GetAttachedData(Url.Parse("file:///a/b/c"), new FileRecord("5", "5", FileType.Directory));
//             Assert.AreEqual(attachedData.Length, 1);
//             Assert.True(!attachedData.Except(new[] { new FileAttachedData("a-1") }).Any());
//
//             await tracker.CommitHint(new DeletedHint(Url.Parse("file:///a/b/c")));
//             AssertWithEvent(
//                 new[]
//                 {
//                     new FileEvent(
//                         FileEvent.EventType.Deleted,
//                         Url.Parse("file:///a/b/c"),
//                         new ("5", "4", FileType.Directory),
//                         new[]
//                         {
//                             new FileAttachedData("a-1"),
//                             new FileAttachedData("d-1", FileAttachedData.DeletionPolicies.WhenFileContentChanged)
//                         }.ToImmutableArray())
//                 });
//         }
//     }
// }
