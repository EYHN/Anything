using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Anything.Database.Orm
{
    public class OrmTransaction : IDbTransaction
    {
        private readonly IDbTransaction _dbTransaction;

        public OrmSystem OrmSystem { get; }

        public bool Completed => _dbTransaction.Completed;

        public ITransaction.TransactionMode Mode => _dbTransaction.Mode;

        /// <summary>
        /// Gets the database provider for the transaction.
        /// </summary>
        public OrmDatabaseProvider DatabaseProvider => OrmSystem.DatabaseProvider;

        /// <summary>
        /// Gets the type manager for the transaction.
        /// </summary>
        public OrmTypeManager TypeManager => OrmSystem.TypeManager;

        /// <summary>
        /// Gets the instance manager for the transaction.
        /// </summary>
        public OrmInstanceManager InstanceManager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrmTransaction"/> class.
        /// </summary>
        /// <param name="ormSystem">the orm system associated with the transaction.</param>
        /// <param name="mode">the transaction mode for the transaction.</param>
        internal OrmTransaction(OrmSystem ormSystem, ITransaction.TransactionMode mode)
        {
            OrmSystem = ormSystem;
            _dbTransaction = DatabaseProvider.StartTransaction(mode);
            InstanceManager = new OrmInstanceManager(this);
        }

        public void CreateDatabase()
        {
            EnsureNotCompleted();

            DatabaseProvider.Create(this);
        }

        public bool TryGetObject(
            long objectId,
            OrmTypeInfo typeInfo,
            [MaybeNullWhen(false)] out object obj)
        {
            EnsureNotCompleted();

            OrmSnapshot? snapshot;

            DatabaseProvider.TryReadObject(this, objectId, typeInfo, out snapshot);

            if (snapshot != null)
            {
                obj = InstanceManager.CreateInstance(typeInfo, objectId, snapshot, out var instanceInfo);

                if (obj is IOrmLifeCycle lifeCycle)
                {
                    lifeCycle.OnDidRestore(
                        new IOrmLifeCycle.OrmLifeCycleDidRestoreEvent
                        {
                            OrmSystem = OrmSystem, Transaction = this, CurrentInstance = instanceInfo
                        });
                }

                return true;
            }
            else
            {
                obj = null;
                return false;
            }
        }

        public object? GetObjectOrDefault(long objectId, OrmTypeInfo typeInfo)
        {
            EnsureNotCompleted();

            TryGetObject(objectId, typeInfo, out var obj);

            return obj;
        }

        public T? GetObjectOrDefault<T>(long objectId)
        {
            EnsureNotCompleted();

            var typeInfo = TypeManager.GetOrmTypeInfo(typeof(T));
            TryGetObject(objectId, typeInfo, out var obj);

            if (obj != null)
            {
                return (T)obj;
            }

            return default;
        }

        public void Update(object obj)
        {
            EnsureNotCompleted();

            if (InstanceManager.TryGetInstanceInfo(obj, out var instanceInfo))
            {
                var lifeCycle = obj is IOrmLifeCycle l ? l : null;

                lifeCycle?.OnWillUpdate(
                    new IOrmLifeCycle.OrmLifeCycleWillUpdateEvent
                    {
                        OrmSystem = OrmSystem, Transaction = this, CurrentInstance = instanceInfo
                    });

                var newSnapshot = instanceInfo.TakeSnapshot();

                var diffResult = instanceInfo.DiffChange(newSnapshot);

                DatabaseProvider.Update(this, instanceInfo.ObjectId, instanceInfo.TypeInfo, diffResult);
                var oldState = instanceInfo.SavedState;
                RunSideEffect(
                    () =>
                    {
                        instanceInfo.SavedState = newSnapshot;
                    },
                    () =>
                    {
                        instanceInfo.SavedState = oldState;
                    });

                lifeCycle?.OnDidUpdate(
                    new IOrmLifeCycle.OrmLifeCycleDidUpdateEvent
                    {
                        OrmSystem = OrmSystem, Transaction = this, CurrentInstance = instanceInfo
                    });
            }
            else
            {
                throw new ArgumentException("The object is not managed.", nameof(obj));
            }
        }

        public void Insert(object obj, long? objectId = null)
        {
            EnsureNotCompleted();

            InstanceManager.TryGetInstanceInfo(obj, out var oldInstanceInfo);

            if (oldInstanceInfo != null)
            {
                throw new ArgumentException("The object has been managed.", nameof(obj));
            }

            var lifeCycle = obj is IOrmLifeCycle l ? l : null;

            lifeCycle?.OnWillCreate(
                new IOrmLifeCycle.OrmLifeCycleWillCreateEvent { OrmSystem = OrmSystem, Transaction = this });

            var typeInfo = TypeManager.GetOrmTypeInfo(obj.GetType());

            var snapshot = OrmInstanceInfo.TakeSnapshot(obj, typeInfo);

            objectId ??= NextObjectId(typeInfo);
            DatabaseProvider.Insert(this, objectId.Value, typeInfo, snapshot);

            var instanceInfo = InstanceManager.RegisterInstance(obj, typeInfo, objectId.Value, snapshot);

            lifeCycle?.OnDidCreate(new IOrmLifeCycle.OrmLifeCycleDidCreateEvent
            {
                OrmSystem = OrmSystem, Transaction = this, CurrentInstance = instanceInfo
            });
        }

        public void Save(object obj)
        {
            EnsureNotCompleted();

            if (InstanceManager.TryGetInstanceInfo(obj, out _))
            {
                Update(obj);
            }
            else
            {
                Insert(obj);
            }
        }

        private void Release(long objectId)
        {
            EnsureNotCompleted();

            DatabaseProvider.Release(this, objectId);
        }

        public void Release(object instance)
        {
            EnsureNotCompleted();

            InstanceManager.TryGetInstanceInfo(instance, out var instanceInfo);

            if (instanceInfo == null)
            {
                throw new InvalidOperationException("Can't release unmanaged instance.");
            }

            Release(instanceInfo.ObjectId);
        }

        public long NextObjectId(OrmTypeInfo typeInfo)
        {
            EnsureNotCompleted();

            return DatabaseProvider.NextObjectId(this, typeInfo);
        }

        public OrmLazy<T?> CreateLazyObject<T>(long objectId)
            where T : class?
        {
            EnsureNotCompleted();

            OrmTypeInfo typeInfo = TypeManager.GetOrmTypeInfo(typeof(T));
            return new(() => (T)GetObjectOrDefault(objectId, typeInfo));
        }

        public object CreateLazyObject(long objectId, OrmTypeInfo typeInfo)
        {
            EnsureNotCompleted();

            var lazyType = OrmLazy.MakeGenericType(typeInfo.TargetType);

            // make lazy provider delegate
            var getMethod = typeof(OrmTransaction).GetMethod(
                nameof(GetObjectOrDefault),
                new[] { typeof(long), typeof(OrmTypeInfo) })!;
            var callExpr = Expression.Call(
                Expression.Constant(this),
                getMethod,
                Expression.Constant(objectId),
                Expression.Constant(typeInfo));
            var convertExpr = Expression.Convert(callExpr, typeInfo.TargetType);
            var @delegate = Expression.Lambda(convertExpr).Compile();

            return Activator.CreateInstance(lazyType, @delegate)!;
        }

        public long GetId(object instance)
        {
            InstanceManager.TryGetInstanceInfo(instance, out var instanceInfo);

            if (instanceInfo == null)
            {
                throw new InvalidOperationException("instance is not managed.");
            }

            return instanceInfo.ObjectId;
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(Func<string> sqlInitializer, string name, params object?[] args)
        {
            return _dbTransaction.ExecuteNonQuery(sqlInitializer, name, args);
        }

        /// <inheritdoc/>
        public T ExecuteReader<T>(Func<string> sqlInitializer, string name, Func<DbDataReader, T> readerFunc, params object?[] args)
        {
            return _dbTransaction.ExecuteReader(sqlInitializer, name, readerFunc, args);
        }

        /// <inheritdoc/>
        public object? ExecuteScalar(
            Func<string> sqlInitializer,
            string name,
            params object?[] args)
        {
            return _dbTransaction.ExecuteScalar(sqlInitializer, name, args);
        }

        /// <inheritdoc/>
        public Task<int> ExecuteNonQueryAsync(
            Func<string> sqlInitializer,
            string name,
            params object?[] args)
        {
            return _dbTransaction.ExecuteNonQueryAsync(sqlInitializer, name, args);
        }

        /// <inheritdoc/>
        public Task<T> ExecuteReaderAsync<T>(
            Func<string> sqlInitializer,
            string name,
            Func<DbDataReader, T> readerFunc,
            params object?[] args)
        {
            return _dbTransaction.ExecuteReaderAsync(sqlInitializer, name, readerFunc, args);
        }

        /// <inheritdoc/>
        public Task<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object?[] args)
        {
            return _dbTransaction.ExecuteScalarAsync(sqlInitializer, name, args);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _dbTransaction.Dispose();
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return _dbTransaction.DisposeAsync();
        }

        /// <inheritdoc/>
        public void PushRollbackWork(Action func)
        {
            _dbTransaction.PushRollbackWork(func);
        }

        /// <inheritdoc/>
        public void RunSideEffect(Action sideEffect, Action rollback)
        {
            _dbTransaction.RunSideEffect(sideEffect, rollback);
        }

        /// <inheritdoc/>
        public T RunSideEffect<T>(Func<T> sideEffect, Action rollback)
        {
            return _dbTransaction.RunSideEffect(sideEffect, rollback);
        }

        /// <inheritdoc/>
        public void DoRollbackWorks()
        {
            _dbTransaction.DoRollbackWorks();
        }

        /// <inheritdoc/>
        public Task CommitAsync()
        {
            return _dbTransaction.CommitAsync();
        }

        /// <inheritdoc/>
        public void Commit()
        {
            _dbTransaction.Commit();
        }

        /// <inheritdoc/>
        public Task RollbackAsync()
        {
            return _dbTransaction.RollbackAsync();
        }

        /// <inheritdoc/>
        public void Rollback()
        {
            _dbTransaction.Rollback();
        }

        public void EnsureNotCompleted()
        {
            if (_dbTransaction.Completed)
            {
                throw new InvalidOperationException("The transaction is completed.");
            }
        }
    }
}
