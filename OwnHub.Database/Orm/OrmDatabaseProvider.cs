using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OwnHub.Database.Orm
{
    public abstract class OrmDatabaseProvider
    {
        public OrmSystem OrmSystem { get; internal set; } = null!;

        public OrmSnapshot? ReadObjectOrDefault(
            OrmTransaction transaction,
            long objectId,
            OrmTypeInfo typeInfo)
        {
            TryReadObject(transaction, objectId, typeInfo, out var snapshot);

            return snapshot;
        }

        public OrmSnapshot ReadObject(
            OrmTransaction transaction,
            long objectId,
            OrmTypeInfo typeInfo)
        {
            if (TryReadObject(transaction, objectId, typeInfo, out var snapshot))
            {
                return snapshot;
            }

            throw new InvalidOperationException("No match object.");
        }

        public abstract void Create(
            OrmTransaction transaction);

        public abstract bool TryReadObject(
            OrmTransaction transaction,
            long objectId,
            OrmTypeInfo typeInfo,
            [MaybeNullWhen(false)] out OrmSnapshot snapshot);

        public abstract void Update(
            OrmTransaction transaction,
            long objectId,
            OrmTypeInfo typeInfo,
            IEnumerable<OrmSnapshot.DiffResult> changing);

        public abstract void Insert(
            OrmTransaction transaction,
            long objectId,
            OrmTypeInfo typeInfo,
            OrmSnapshot snapshot);

        /// <summary>
        /// Release an object. Usually triggered by the database provider calling <see cref="OwnHub.Database.Orm.OrmTransaction.Release"/> , then the database should delete the object.
        /// If the changed object contains other objects, you need to call <see cref="OwnHub.Database.Orm.OrmTransaction.Release"/> recursively.
        /// </summary>
        /// <param name="transaction">database transaction.</param>
        /// <param name="objectId">id of the object.</param>
        public abstract void Release(OrmTransaction transaction, long objectId);

        public abstract long NextObjectId(OrmTransaction transaction, OrmTypeInfo typeInfo);

        public abstract IDbTransaction StartTransaction(ITransaction.TransactionMode mode);
    }
}
