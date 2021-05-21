using Anything.Database;
using Anything.Database.Orm;
using Anything.Database.Triples;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Database.Triples
{
    public class MovieDatabaseTests
    {
        [Test]
        public void ShortMovieDatabaseTests()
        {
            var context = TestUtils.CreateSqliteContext();
            var db = TriplesDatabase.Create(context, "triples");
            db.RegisteredType(typeof(Movie));

            using var t = db.StartTransaction(ITransaction.TransactionMode.Mutation);

            var movies = Resources.ReadEmbeddedJsonFile<Movie[]>(typeof(MovieDatabaseTests).Assembly, "Resources/movie-data-short.json");

            for (var i = 0; i < movies.Length; i++)
            {
                t.Root.SetAndSave($"movies[{i}]", movies[i]);
            }

            t.Commit();
        }

        [Test]
        public void LargeMovieDatabaseTests()
        {
            var context = TestUtils.CreateSqliteContext();
            var db = TriplesDatabase.Create(context, "triples");
            db.RegisteredType(typeof(Movie));

            using var t = db.StartTransaction(ITransaction.TransactionMode.Mutation);

            var movies = Resources.ReadEmbeddedJsonFile<Movie[]>(typeof(MovieDatabaseTests).Assembly, "Resources/movie-data.json");

            for (var i = 0; i < movies.Length; i++)
            {
                t.Root.SetAndSave($"movies[{i}]", movies[i]);
            }

            t.Commit();
        }

        [OrmType]
        public class Movie
        {
#pragma warning disable 8618
            [OrmProperty]
            public string Title { get; set; }

            [OrmProperty]
            public string Year { get; set; }

            [OrmProperty]
            public string Rated { get; set; }

            [OrmProperty]
            public string Released { get; set; }

            [OrmProperty]
            public string Runtime { get; set; }

            [OrmProperty]
            public string Genre { get; set; }

            [OrmProperty]
            public string Director { get; set; }

            [OrmProperty]
            public string Writer { get; set; }

            [OrmProperty]
            public string Actors { get; set; }

            [OrmProperty]
            public string Plot { get; set; }

            [OrmProperty]
            public string Language { get; set; }

            [OrmProperty]
            public string Country { get; set; }

            [OrmProperty]
            public string Awards { get; set; }

            [OrmProperty]
            public string Poster { get; set; }

            [OrmProperty]
            public string Metascore { get; set; }

            [OrmProperty]
            public string ImdbRating { get; set; }

            [OrmProperty]
            public string ImdbVotes { get; set; }

            [OrmProperty]
            public string ImdbId { get; set; }

            [OrmProperty]
            public string Type { get; set; }

            [OrmProperty]
            public string Dvd { get; set; }

            [OrmProperty]
            public string BoxOffice { get; set; }

            [OrmProperty]
            public string Production { get; set; }

            [OrmProperty]
            public string Website { get; set; }

            [OrmProperty]
            public string Response { get; set; }
#pragma warning restore 8618
        }
    }
}
