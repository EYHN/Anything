using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace OwnHub.Sqlite.Triples
{
    [TriplesTypeName("Root")]
    public sealed class TriplesRoot : TriplesObject
    {
        public void SetChild(string name, object value) => SetProperty(name, value);

        public void SetChild(string name, object value, TriplesTransaction transaction) => SetProperty(name, value, transaction);

        public bool TryGetChild<T>(string name, [MaybeNullWhen(false)] out T obj) => TryGetProperty(name, out obj);

        public T? GetChild<T>(string name) => GetProperty<T>(name);

        public void DeleteChild(string name) => DeleteProperty(name);

        public ValueTask SetChildAsync(string name, object value) => SetPropertyAsync(name, value);

        public ValueTask<T?> GetChildAsync<T>(string name) => GetPropertyAsync<T>(name);

        public ValueTask<object?> GetChildAsync(string name) => GetPropertyAsync(name);

        public ValueTask DeleteChildAsync(string name) => DeletePropertyAsync(name);

        internal TriplesRoot(TriplesDatabase database)
        {
            Id = 0;
            Status = ObjectStatus.Managed;
            Database = database;
        }
    }
}
