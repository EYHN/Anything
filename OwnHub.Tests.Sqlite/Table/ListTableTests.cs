using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class ListTableTests
    {
        public SqliteContext CreateSqliteContext(string name)
        {
            return new SqliteContext(new SharedMemoryConnectionProvider("ListTableTests-" + name));
        }
        
        [Test]
        public async Task FeatureTest()
        {
            SqliteContext context = CreateSqliteContext("FeatureTest");
            ListTable table = new ListTable(context, "ListTable");
            await table.Create();

            long id1 = await table.Insert("hello");
            long id2 = await table.Insert("world");

            Assert.AreEqual(await table.Search(id1), "hello");
            Assert.AreEqual(await table.Search(id2), "world");
        }
    }
}