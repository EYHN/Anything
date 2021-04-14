using System.Collections.Generic;
using OwnHub.Database;
using OwnHub.Database.Orm;

namespace OwnHub.Tests.Database.Orm
{
    public class TestDatabaseProvider : OrmDatabaseProvider
    {
        public record ObjectRef(long Id);

        private Dictionary<long, Dictionary<string, object?>> _data;

        public TestDatabaseProvider(Dictionary<long, Dictionary<string, object?>> init)
        {
            _data = init;
        }

        public override void Create(OrmTransaction transaction)
        {
        }

        public override bool TryReadObject(OrmTransaction transaction, long objectId, OrmTypeInfo typeInfo, out OrmSnapshot snapshot)
        {
            snapshot = new OrmSnapshot();

            if (_data.TryGetValue(objectId, out var obj))
            {
                foreach (var property in typeInfo.Properties)
                {
                    var name = property.Name;
                    var type = property.ValueTypeInfo;
                    var value = obj[name];

                    if (value is ObjectRef objectRef)
                    {
                        value = transaction.GetObjectOrDefault(objectRef.Id, type);
                    }

                    snapshot.Add(property, value);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Update(
            OrmTransaction transaction,
            long objectId,
            OrmTypeInfo typeInfo,
            IEnumerable<OrmSnapshot.DiffResult> changing)
        {
            throw new System.NotImplementedException();
        }

        public override void Insert(OrmTransaction transaction, long objectId, OrmTypeInfo typeInfo, OrmSnapshot snapshot)
        {
            throw new System.NotImplementedException();
        }

        public override void Release(OrmTransaction transaction, long objectId)
        {
            throw new System.NotImplementedException();
        }

        public override long NextObjectId(OrmTransaction transaction, OrmTypeInfo typeInfo)
        {
            throw new System.NotImplementedException();
        }

        public override IDbTransaction StartTransaction(ITransaction.TransactionMode mode)
        {
            throw new System.NotImplementedException();
        }
    }
}
