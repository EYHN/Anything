using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Triples;
using OwnHub.Utils;

namespace OwnHub.Tests.Sqlite.Triples
{
    public class TriplesJsonObjectTests
    {
        [Test]
        public void ShortMovieDatabaseTest()
        {
            var context = TestUtils.CreateSqliteContext("ShortMovieDatabase");
            var database = new TriplesDatabase(context, "Triples");
            database.RegisterObjectType<TriplesJsonObject>();
            database.Create();

            var document = Resources.ReadEmbeddedJsonFile(typeof(TriplesJsonObjectTests).Assembly, "Resources/movie-data-short.json");
            var index = 0;
            TriplesTransaction transaction = new (database, Transaction.TransactionMode.Mutation);
            foreach (var item in document.RootElement.EnumerateArray())
            {
                var obj = new TriplesJsonObject(item);
                database.Root.SetChild($"movie[{index++}]", obj, transaction);
            }

            transaction.Commit();
        }

        [Test]
        public void MovieDatabaseTest()
        {
            var context = TestUtils.CreateSqliteContext("MovieDatabase");
            var database = new TriplesDatabase(context, "Triples");
            database.RegisterObjectType<TriplesJsonObject>();
            database.Create();

            var document = Resources.ReadEmbeddedJsonFile(typeof(TriplesJsonObjectTests).Assembly, "Resources/movie-data.json");
            var index = 0;
            TriplesTransaction transaction = new (database, Transaction.TransactionMode.Mutation);
            foreach (var item in document.RootElement.EnumerateArray())
            {
                var obj = new TriplesJsonObject(item);
                database.Root.SetChild($"movie[{index++}]", obj, transaction);
            }

            transaction.Commit();
        }
    }
}
