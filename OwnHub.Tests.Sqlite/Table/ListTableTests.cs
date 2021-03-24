using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class ListTableTests
    {
        public static SqliteContext CreateSqliteContext(string name)
        {
            return new (new SharedMemoryConnectionProvider("ListTableTests-" + name));
        }

        [Test]
        public async Task FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext("FeatureTest");
            var table = new ListTable(context, "ListTable");
            await table.CreateAsync();

            var id1 = await table.InsertAsync("hello");
            var id2 = await table.InsertAsync("world");

            Assert.AreEqual(await table.SearchAsync(id1), "hello");
            Assert.AreEqual(await table.SearchAsync(id2), "world");
        }
    }
}
