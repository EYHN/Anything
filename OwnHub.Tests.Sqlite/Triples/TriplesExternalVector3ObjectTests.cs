using System.Numerics;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Triples;

namespace OwnHub.Tests.Sqlite.Triples
{
    public class TriplesExternalVector3ObjectTests
    {

        [Test]
        public void FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext("FeatureTest");
            var database = new TriplesDatabase(context, "Triples");
            database.RegisterObjectType<TriplesExternalVector3Object>();
            database.Create();

            TriplesExternalVector3Object vector3Object = new(new(1, 2, 3));

            database.Root.SetChild("vector3-test", vector3Object);

            vector3Object.Set(new(3, 2, 1));

            database.Root.TryGetChild("vector3-test", out TriplesExternalVector3Object? offerObject);

            Assert.AreEqual(new Vector3(3, 2, 1), offerObject?.Get());
        }
    }
}
