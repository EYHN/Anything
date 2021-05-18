using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace StagingBox.Database.Orm
{
    public class OrmSystem
    {
        public OrmDatabaseProvider DatabaseProvider { get; }

        public OrmTypeManager TypeManager { get; } = new();

        public OrmSystem(OrmDatabaseProvider databaseProvider)
        {
            DatabaseProvider = databaseProvider;
            DatabaseProvider.OrmSystem = this;
        }

        public virtual void RegisteredType(Type type, string? name = null)
        {
            TypeManager.RegisterType(type, name);
        }

        public virtual void RegisteredScalar(Type type, string? name = null)
        {
            TypeManager.RegisterScalar(type, name);
        }

        public virtual OrmTransaction StartTransaction(ITransaction.TransactionMode mode, Type? customTransactionType = null)
        {
            var transactionType = customTransactionType ?? typeof(OrmTransaction);
            var ctor = transactionType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null,
                new[] { typeof(OrmSystem), typeof(ITransaction.TransactionMode) },
                null);


            return ((OrmTransaction)ctor!.Invoke(new object?[] { this, mode }))!;
        }
    }
}
