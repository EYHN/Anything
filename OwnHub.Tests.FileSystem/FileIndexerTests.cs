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
            HashSet<string> createdFiles = new();
            HashSet<string> changedFiles = new();
            HashSet<string> deletedFiles = new();

            indexer.OnFileChange += @event =>
            {
                foreach (var created in @event.Created)
                {
                    createdFiles.Add(created);
                }

                foreach (var deleted in @event.Deleted)
                {
                    deletedFiles.Add(deleted);
                }

                foreach (var changed in @event.Changed)
                {
                    changedFiles.Add(changed);
                }
            };


            void AssertWithEvent(string[] expectedCreated, string[] expectedChanged, string[] expectedDeleted)
            {
                Assert.IsTrue(expectedCreated.Length == createdFiles.Count && expectedCreated.All(path => createdFiles.Contains(path)));
                Assert.IsTrue(expectedChanged.Length == changedFiles.Count && expectedChanged.All(path => changedFiles.Contains(path)));
                Assert.IsTrue(expectedDeleted.Length == deletedFiles.Count && expectedDeleted.All(path => deletedFiles.Contains(path)));
                createdFiles.Clear();
                changedFiles.Clear();
                deletedFiles.Clear();
            }

            await indexer.IndexFile("/a/b/c", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b/c" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexFile("/a/b", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexFile("/a/b/c/d", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b/c/d" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexFile("/a/b/c", new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(Array.Empty<string>(), new[] { "/a/b/c" }, Array.Empty<string>());

            await indexer.IndexFile("/a/b/c/d", new FileRecord("1", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(Array.Empty<string>(), new[] { "/a/b/c/d" }, Array.Empty<string>());

            await indexer.IndexFile("/a/b/c/d", new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b/c/d" }, Array.Empty<string>(), new[] { "/a/b/c/d" });

            await indexer.IndexFile("/a/b/c", new FileRecord("2", "2", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b/c" }, Array.Empty<string>(), new[] { "/a/b/c", "/a/b/c/d" });

            await indexer.IndexFile("/a/b/c/d", new FileRecord("2", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b/c/d" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexFile("/a/b/c", new FileRecord("3", "2", FileType.File, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b/c" }, Array.Empty<string>(), new[] { "/a/b/c", "/a/b/c/d" });

            await indexer.IndexFile("/a/b/c/e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now));
            AssertWithEvent(new[] { "/a/b/c/e" }, Array.Empty<string>(), new[] { "/a/b/c" });

            await indexer.IndexDirectory(
                "/a/b/c",
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(new[] { "/a/b/c/f" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexDirectory(
                "/abc",
                new[]
                {
                    ("h", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("i", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(new[] { "/abc/h", "/abc/i" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexDirectory(
                "/a/b/c/f",
                new[]
                {
                    ("j", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("k", new FileRecord("1", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(new[] { "/a/b/c/f/j", "/a/b/c/f/k" }, Array.Empty<string>(), new[] { "/a/b/c/f" });

            await indexer.IndexDirectory(
                "/a/b/c",
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("2", "1", FileType.Directory, DateTimeOffset.Now)),
                });
            AssertWithEvent(new[] { "/a/b/c/f" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexDirectory(
                "/a/b/c",
                new[]
                {
                    ("e", new FileRecord("1", "1", FileType.Directory, DateTimeOffset.Now)),
                    ("f", new FileRecord("3", "1", FileType.File, DateTimeOffset.Now)),
                });
            AssertWithEvent(new[] { "/a/b/c/f" }, Array.Empty<string>(), new[] { "/a/b/c/f", "/a/b/c/f/j", "/a/b/c/f/k" });

            await indexer.IndexDirectory(
                "/a/b/c",
                new[] { ("e", new FileRecord("1", "2", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(Array.Empty<string>(), new[] { "/a/b/c/e" }, new[] { "/a/b/c/f" });

            await indexer.IndexDirectory(
                "/a/b",
                new[] { ("c", new FileRecord("4", "2", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(new[] { "/a/b/c" }, Array.Empty<string>(), Array.Empty<string>());

            await indexer.IndexDirectory(
                "/a/b",
                new[] { ("c", new FileRecord("4", "3", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(Array.Empty<string>(), new[] { "/a/b/c" }, Array.Empty<string>());

            await indexer.IndexDirectory(
                "/a/b",
                new[] { ("c", new FileRecord("5", "3", FileType.Directory, DateTimeOffset.Now)), });
            AssertWithEvent(new[] { "/a/b/c" }, Array.Empty<string>(), new[] { "/a/b/c", "/a/b/c/e" });
        }
    }
}
