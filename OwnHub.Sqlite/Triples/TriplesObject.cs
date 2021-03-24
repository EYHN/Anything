using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace OwnHub.Sqlite.Triples
{
    public abstract partial class TriplesObject
    {
        public enum ObjectStatus
        {
            /// <summary>
            /// When the object is initialized by the user using the new operator, the object is not currently in the database.
            /// At this point all reads and writes of properties occur in the cache.
            /// </summary>
            Pending = 1,

            /// <summary>
            /// When the object is saved to the database, all properties are sync with the database.
            /// </summary>
            Managed = 2,

            /// <summary>
            /// When the object is released, the object has been deleted from the database.
            /// </summary>
            Released = 3
        }

        public long? Id { get; protected set; }

        /// <summary>
        /// Gets or sets current status of the object.
        /// </summary>
        public ObjectStatus Status { get; set; } = ObjectStatus.Pending;

        public TriplesDatabase? Database { get; protected set; }

        protected Transaction BeginMutationTransaction()
        {
            return Database != null
                ? new TriplesTransaction(Database, Transaction.TransactionMode.Mutation)
                : new Transaction(Transaction.TransactionMode.Mutation);
        }

        protected Transaction BeginQueryTransaction()
        {
            return Database != null
                ? new TriplesTransaction(Database, Transaction.TransactionMode.Query)
                : new Transaction(Transaction.TransactionMode.Query);
        }

        protected async ValueTask SetPropertyAsync(string name, object value)
        {
            await using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        SetPropertyCache(name, value, transaction);
                        break;
                    case ObjectStatus.Managed:
                        await SetPropertyDatabaseAsync(name, value, (TriplesTransaction)transaction);
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

        protected async ValueTask SetPropertyAsync(string name, object value, TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    SetPropertyCache(name, value, transaction);
                    break;
                case ObjectStatus.Managed:
                    await SetPropertyDatabaseAsync(name, value, transaction);
                    break;
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected void SetProperty(string name, object value)
        {
            using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        SetPropertyCache(name, value, transaction);
                        break;
                    case ObjectStatus.Managed:
                        SetPropertyDatabase(name, value, (TriplesTransaction)transaction);
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

        protected void SetProperty(string name, object value, TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    SetPropertyCache(name, value, transaction);
                    break;
                case ObjectStatus.Managed:
                    SetPropertyDatabase(name, value, transaction);
                    break;
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected async ValueTask<object?> GetPropertyAsync(string name)
        {
            await using var transaction = BeginQueryTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        TryGetPropertyCache(name, out var obj);
                        return obj;
                    case ObjectStatus.Managed:
                        return await GetPropertyDatabaseAsync(name, (TriplesTransaction)transaction);
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

        protected async ValueTask<object?> GetPropertyAsync(string name, TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    TryGetPropertyCache(name, out var obj);
                    return obj;
                case ObjectStatus.Managed:
                    return await GetPropertyDatabaseAsync(name, transaction);
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected bool TryGetProperty(string name, [MaybeNullWhen(false)] out object obj)
        {
            using var transaction = BeginQueryTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        return TryGetPropertyCache(name, out obj);
                    case ObjectStatus.Managed:
                        return TryGetPropertyDatabase(name, out obj, (TriplesTransaction)transaction);
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

        protected bool TryGetProperty(string name, [MaybeNullWhen(false)] out object obj, TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    return TryGetPropertyCache(name, out obj);
                case ObjectStatus.Managed:
                    return TryGetPropertyDatabase(name, out obj, transaction);
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected bool TryGetProperty<T>(string name, [MaybeNullWhen(false)] out T? obj)
        {
            TryGetProperty(name, out var value);
            obj = value != null ? (T?)value : default;
            return obj != null;
        }

        protected bool TryGetProperty<T>(string name, [MaybeNullWhen(false)] out T? obj, TriplesTransaction transaction)
        {
            TryGetProperty(name, out var value, transaction);
            obj = value != null ? (T?)value : default;
            return obj != null;
        }

        protected object? GetProperty(string name)
        {
            TryGetProperty(name, out var value);
            return value;
        }

        protected object? GetProperty(string name, TriplesTransaction transaction)
        {
            TryGetProperty(name, out var value, transaction);
            return value;
        }

        protected T? GetProperty<T>(string name)
        {
            TryGetProperty(name, out var value);
            return value != null ? (T)value : default;
        }

        protected T? GetProperty<T>(string name, TriplesTransaction transaction)
        {
            TryGetProperty(name, out var value, transaction);
            return value != null ? (T)value : default;
        }

        protected async ValueTask<T?> GetPropertyAsync<T>(string name)
        {
            var result = await GetPropertyAsync(name);
            return result != null ? (T)result : default;
        }

        protected async ValueTask<T?> GetPropertyAsync<T>(string name, TriplesTransaction transaction)
        {
            var result = await GetPropertyAsync(name, transaction);
            return result != null ? (T)result : default;
        }

        protected async ValueTask DeletePropertyAsync(string name)
        {
            await using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        DeletePropertyCache(name, transaction);
                        break;
                    case ObjectStatus.Managed:
                        await DeletePropertyDatabaseAsync(name, (TriplesTransaction)transaction);
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

        protected async ValueTask DeletePropertyAsync(string name, TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    DeletePropertyCache(name, transaction);
                    break;
                case ObjectStatus.Managed:
                    await DeletePropertyDatabaseAsync(name, transaction);
                    break;
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected void DeleteProperty(string name)
        {
            using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        DeletePropertyCache(name, transaction);
                        break;
                    case ObjectStatus.Managed:
                        DeletePropertyDatabase(name, (TriplesTransaction)transaction);
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

        protected void DeleteProperty(string name, TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    DeletePropertyCache(name, transaction);
                    break;
                case ObjectStatus.Managed:
                    DeletePropertyDatabase(name, transaction);
                    break;
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected async ValueTask<IEnumerable<KeyValuePair<string, object>>> GetAllPropertiesAsync()
        {
            await using var transaction = BeginQueryTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        return GetAllPropertiesCache();
                    case ObjectStatus.Managed:
                        return await GetAllPropertiesDatabaseAsync((TriplesTransaction)transaction);
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

        protected async ValueTask<IEnumerable<KeyValuePair<string, object>>> GetAllPropertiesAsync(TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    return GetAllPropertiesCache();
                case ObjectStatus.Managed:
                    return await GetAllPropertiesDatabaseAsync(transaction);
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected IEnumerable<KeyValuePair<string, object>> GetAllProperties()
        {
            using var transaction = BeginQueryTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        return GetAllPropertiesCache();
                    case ObjectStatus.Managed:
                        return GetAllPropertiesDatabase((TriplesTransaction)transaction);
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

        protected IEnumerable<KeyValuePair<string, object>> GetAllProperties(TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    return GetAllPropertiesCache();
                case ObjectStatus.Managed:
                    return GetAllPropertiesDatabase(transaction);
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected async ValueTask DeleteAllPropertiesAsync()
        {
            await using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        DeleteAllPropertiesCache(transaction);
                        break;
                    case ObjectStatus.Managed:
                        await DeleteAllPropertiesDatabaseAsync((TriplesTransaction)transaction);
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

        protected async ValueTask DeleteAllPropertiesAsync(TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    DeleteAllPropertiesCache(transaction);
                    break;
                case ObjectStatus.Managed:
                    await DeleteAllPropertiesDatabaseAsync(transaction);
                    break;
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected void DeleteAllProperties()
        {
            using var transaction = BeginMutationTransaction();
            try
            {
                switch (Status)
                {
                    case ObjectStatus.Pending:
                        DeleteAllPropertiesCache(transaction);
                        break;
                    case ObjectStatus.Managed:
                        DeleteAllPropertiesDatabase((TriplesTransaction)transaction);
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

        protected void DeleteAllProperties(TriplesTransaction transaction)
        {
            switch (Status)
            {
                case ObjectStatus.Pending:
                    DeleteAllPropertiesCache(transaction);
                    break;
                case ObjectStatus.Managed:
                    DeleteAllPropertiesDatabase(transaction);
                    break;
                case ObjectStatus.Released:
                    throw new InvalidOperationException("Object has released.");
                default:
                    throw new InvalidOperationException("Unknown status");
            }
        }

        protected virtual void Create(TriplesTransaction transaction)
        {
        }

        protected virtual void Release(TriplesTransaction transaction)
        {
        }

        protected virtual ValueTask CreateAsync(TriplesTransaction transaction)
        {
            return default;
        }

        protected virtual ValueTask ReleaseAsync(TriplesTransaction transaction)
        {
            return default;
        }

        internal static object RestoreFromDatabase(Type objectType, TriplesDatabase database, long id)
        {
            var instance = Activator.CreateInstance(
                objectType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null,
                null,
                null,
                null);
            if (instance is TriplesObject triplesObject)
            {
                triplesObject.Id = id;
                triplesObject.Database = database;
                triplesObject.Status = ObjectStatus.Managed;
                return triplesObject;
            }
            else
            {
                throw new InvalidOperationException("Can't create instance of type: " + objectType.Name);
            }
        }
    }
}
