using System;
using System.Threading.Tasks;

namespace OwnHub.Sqlite.Triples
{
    public abstract class TriplesExternalScalar<T> : TriplesExternalObject
        where T : struct
    {
        private T? _cache = default;

        protected void SetScalar(T scalar)
        {
            using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        SetScalarCache(scalar, transaction);
                        break;
                    case ObjectStatus.Managed:
                        UpdateScalarDatabase(scalar, (TriplesTransaction)transaction);
                        break;
                    case ObjectStatus.Released:
                        throw new InvalidOperationException("Object has released.");
                    default:
                        throw new InvalidOperationException("Unknown status");
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        protected async ValueTask SetScalarAsync(T scalar)
        {
            await using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        SetScalarCache(scalar, transaction);
                        break;
                    case ObjectStatus.Managed:
                        await UpdateScalarDatabaseAsync(scalar, (TriplesTransaction)transaction);
                        break;
                    case ObjectStatus.Released:
                        throw new InvalidOperationException("Object has released.");
                    default:
                        throw new InvalidOperationException("Unknown status");
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        protected T? GetScalar()
        {
            using var transaction = BeginQueryTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        return GetScalarCache();
                    case ObjectStatus.Managed:
                        return GetScalarDatabase((TriplesTransaction)transaction);
                    case ObjectStatus.Released:
                        throw new InvalidOperationException("Object has released.");
                    default:
                        throw new InvalidOperationException("Unknown status");
                }
            }
            finally
            {
                transaction.Commit();
            }
        }

        protected async ValueTask<T?> GetScalarAsync()
        {
            await using var transaction = BeginQueryTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        return GetScalarCache();
                    case ObjectStatus.Managed:
                        return await GetScalarDatabaseAsync((TriplesTransaction)transaction);
                    case ObjectStatus.Released:
                        throw new InvalidOperationException("Object has released.");
                    default:
                        throw new InvalidOperationException("Unknown status");
                }
            }
            finally
            {
                await transaction.CommitAsync();
            }
        }

        private T? GetScalarDatabase(TriplesTransaction transaction)
        {
            return Read(transaction);
        }

        private async ValueTask<T?> GetScalarDatabaseAsync(TriplesTransaction transaction)
        {
            return await ReadAsync(transaction);
        }

        private void UpdateScalarDatabase(T scalar, TriplesTransaction transaction)
        {
            Update(transaction, scalar);
        }

        private async ValueTask UpdateScalarDatabaseAsync(T scalar, TriplesTransaction transaction)
        {
            await UpdateAsync(transaction, scalar);
        }

        private void SetScalarCache(T? scalar, Transaction transaction)
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            var oldScalar = _cache;
            transaction.RunSideEffect(
                () =>
                {
                    _cache = scalar;
                },
                () =>
                {
                    _cache = oldScalar;
                });
        }

        private T? GetScalarCache()
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            return _cache;
        }

        protected abstract T? Read(TriplesTransaction transaction);

        protected abstract ValueTask<T?> ReadAsync(TriplesTransaction transaction);

        protected abstract void Save(TriplesTransaction transaction, T scalar);

        protected abstract ValueTask SaveAsync(TriplesTransaction transaction, T scalar);

        protected abstract void Update(TriplesTransaction transaction, T scalar);

        protected abstract ValueTask UpdateAsync(TriplesTransaction transaction, T scalar);

        protected override void Create(TriplesTransaction transaction)
        {
            if (_cache != null)
            {
                Save(transaction, _cache.Value);
            }
            else
            {
                throw new InvalidOperationException("The scalar can't be null.");
            }
        }

        protected override async ValueTask CreateAsync(TriplesTransaction transaction)
        {
            if (_cache != null)
            {
                await SaveAsync(transaction, _cache.Value);
            }
            else
            {
                throw new InvalidOperationException("The scalar can't be null.");
            }
        }
    }
}
