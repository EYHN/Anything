using System.Numerics;
using System.Threading.Tasks;
using OwnHub.Sqlite.Table;

namespace OwnHub.Sqlite.Triples
{
    [TriplesExternalTable(typeof(Vector3IndexTable), "Vector3External")]
    public class TriplesExternalVector3Object : TriplesExternalScalar<Vector3>
    {
        private Vector3IndexTable Table => Database!.GetExternalTable<Vector3IndexTable>();

        protected TriplesExternalVector3Object()
        {
        }

        public TriplesExternalVector3Object(Vector3 v)
        {
            SetScalar(v);
        }

        public void Set(Vector3 v)
        {
            SetScalar(v);
        }

        public Vector3? Get()
        {
            return GetScalar();
        }

        public async ValueTask SetAsync(Vector3 v)
        {
            await SetScalarAsync(v);
        }

        public async ValueTask<Vector3?> GetAsync()
        {
            return await GetScalarAsync();
        }

        protected override void Save(TriplesTransaction transaction, Vector3 scalar)
        {
            Table.Insert(transaction.DbConnection, Id!.Value, scalar);
        }

        protected override async ValueTask SaveAsync(TriplesTransaction transaction, Vector3 scalar)
        {
            await Table.InsertAsync(transaction.DbConnection, Id!.Value, scalar);
        }

        protected override void Update(TriplesTransaction transaction, Vector3 scalar)
        {
            Table.Update(transaction.DbConnection, Id!.Value, scalar);
        }

        protected override async ValueTask UpdateAsync(TriplesTransaction transaction, Vector3 scalar)
        {
            await Table.UpdateAsync(transaction.DbConnection, Id!.Value, scalar);
        }

        protected override Vector3? Read(TriplesTransaction transaction)
        {
            return Table.Select(transaction.DbConnection, Id!.Value)?.V;
        }

        protected override async ValueTask<Vector3?> ReadAsync(TriplesTransaction transaction)
        {
            return (await Table.SelectAsync(transaction.DbConnection, Id!.Value))?.V;
        }

        protected override void Release(TriplesTransaction transaction)
        {
            Table.Delete(transaction.DbConnection, Id!.Value);
        }

        protected override async ValueTask ReleaseAsync(TriplesTransaction transaction)
        {
            await Table.DeleteAsync(transaction.DbConnection, Id!.Value);
        }
    }
}
