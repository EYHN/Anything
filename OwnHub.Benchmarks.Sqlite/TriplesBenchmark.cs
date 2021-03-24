using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Triples;
using OwnHub.Utils;

namespace OwnHub.Benchmarks.Sqlite
{
    public class TriplesBenchmark
    {
        [TriplesTypeName("JSON")]
        public class TriplesJsonObject : TriplesObject
        {
            public object? this[string name]
            {
                get => GetProperty(name);
                set
                {
                    if (value != null)
                    {
                        SetProperty(name, value);
                    }
                    else
                    {
                        DeleteProperty(name);
                    }
                }
            }
        }

        private SqliteContext CreateSqliteContext()
        {
            return new (new SqliteConnectionProvider("TriplesBenchmark"));
        }

        public TriplesBenchmark()
        {
            var context = CreateSqliteContext();
            _database = new TriplesDatabase(context, "Triples");
            _database.RegisterObjectType<TriplesJsonObject>();
            _database.Create();

            var json = Resources.ReadEmbeddedJsonFile(typeof(TriplesBenchmark).Assembly, "Resources/movie-data.json");
            _list = json.RootElement.EnumerateArray().ToList();
        }

        private readonly TriplesDatabase _database;
        private int _index = 0;
        private readonly List<JsonElement> _list;

        [Benchmark]
        public void Write()
        {
            var triplesObject = new TriplesJsonObject();
            foreach (var property in _list[_index % _list.Count].EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    triplesObject[property.Name] = property.Value.GetString();
                }
            }

            _database.Root.SetChild("" + _index++, triplesObject);
        }
    }
}
