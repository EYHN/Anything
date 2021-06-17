using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem;
using Anything.Search;
using Anything.Search.Modules.FileName;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Search.Modules.FileName
{
    public class FileNameTests
    {
        [Test]
        public async Task FileNameSearchTest()
        {
            var sqliteContext = TestUtils.CreateSqliteContext();
            var memoryFileService = await FileServiceFactory.BuildMemoryFileService();
            var fileNameSearch = new FileNameSearch(sqliteContext, memoryFileService);

            await memoryFileService.FileSystem.WriteFile(Url.Parse("file://test/foobar"), new byte[0]);
            await memoryFileService.FileSystem.CreateDirectory(Url.Parse("file://test/foo"));
            await memoryFileService.FileSystem.WriteFile(Url.Parse("file://test/foo/bar"), new byte[0]);
            {
                var result = await fileNameSearch.Search(new SearchOption("bar", null));
                Assert.AreEqual(2, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foobar")));
                Assert.True(result.Items.Contains(Url.Parse("file://test/foo/bar")));
            }

            {
                var result = await fileNameSearch.Search(new SearchOption("foo", null));
                Assert.AreEqual(2, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foobar")));
                Assert.True(result.Items.Contains(Url.Parse("file://test/foo")));
            }

            {
                var result = await fileNameSearch.Search(new SearchOption("bar", Url.Parse("file://test/foo")));
                Assert.AreEqual(1, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foo/bar")));
            }

            await memoryFileService.FileSystem.Delete(Url.Parse("file://test/foo"), true);
            {
                var result = await fileNameSearch.Search(new SearchOption("bar", null));
                Assert.AreEqual(1, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foobar")));
            }
        }

        [Test]
        public async Task FileNameTableTest()
        {
            var sqliteContext = TestUtils.CreateSqliteContext();
            var table = new FileNameIndexer.FileNameTable("FileName");

            await using var transaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Create);
            await table.CreateAsync(transaction);
            await table.InsertAsync(transaction, "foobar", Url.Parse("file://test/foobar"));
            await table.InsertAsync(transaction, "bar", Url.Parse("file://test/foo/bar"));
            {
                var urls = await table.SearchAsync(transaction, "bar", Url.Parse("file://test/"));
                Assert.AreEqual(2, urls.Length);
                Assert.True(urls.Contains(Url.Parse("file://test/foobar")));
                Assert.True(urls.Contains(Url.Parse("file://test/foo/bar")));
            }

            {
                var urls = await table.SearchAsync(transaction, "bar", Url.Parse("file://test/foo"));
                Assert.AreEqual(1, urls.Length);
                Assert.True(urls.Contains(Url.Parse("file://test/foo/bar")));
            }

            {
                var urls = await table.SearchAsync(transaction, "bar", null);
                Assert.AreEqual(2, urls.Length);
                Assert.True(urls.Contains(Url.Parse("file://test/foobar")));
                Assert.True(urls.Contains(Url.Parse("file://test/foo/bar")));
            }

            {
                await table.DeleteAsync(transaction, Url.Parse("file://test/foobar"));
                var urls = await table.SearchAsync(transaction, "bar", null);
                Assert.AreEqual(1, urls.Length);
                Assert.True(urls.Contains(Url.Parse("file://test/foo/bar")));
            }

            await transaction.CommitAsync();
        }
    }
}
