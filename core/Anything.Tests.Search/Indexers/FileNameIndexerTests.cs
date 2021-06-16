using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Search.Indexers;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Search.Indexers
{
    public class FileNameIndexerTests
    {
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
