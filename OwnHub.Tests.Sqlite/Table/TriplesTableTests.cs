using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class TriplesTableTests
    {
        public SqliteContext CreateSqliteContext(string name)
        {
            return new SqliteContext(new SharedMemoryConnectionProvider("TriplesTableTests-" + name));
        }
        
        [Test]
        public async Task FeatureTest()
        {
            SqliteContext context = CreateSqliteContext("FeatureTest");
            TriplesTable table = new TriplesTable(context, "TriplesTable");
            await table.Create();

            await table.Insert("subject1", "predicate1", 1);
            await table.Insert("subject1", "predicate2", 0.5);
            await table.Insert("subject2", "predicate3", "hello");
            await table.Insert("subject2", "predicate4", new DateTimeOffset(DateTime.UnixEpoch));
            await table.Insert("subject3", "predicate5", (object?) null);

            TriplesTable.Row? row = await table.Search("subject1", "predicate1");
            Assert.AreEqual(row?.Obj, 1);
            
            row = await table.Search("subject1", "predicate2");
            Assert.AreEqual(row?.Obj, 0.5);
            
            row = await table.Search("subject2", "predicate3");
            Assert.AreEqual(row?.Obj, "hello");
            
            row = await table.Search("subject2", "predicate4");
            Assert.AreEqual(row?.Obj, new DateTimeOffset(DateTime.UnixEpoch));
            
            row = await table.Search("subject3", "predicate5");
            Assert.IsNull(row?.Obj);

            row = await table.Search(row!.Id);
            Assert.AreEqual(row?.Subject, "subject3");
            Assert.AreEqual(row?.Predicate, "predicate5");
            Assert.IsNull(row?.Obj);
        }
    }
}