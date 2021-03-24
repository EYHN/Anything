using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OwnHub.Sqlite.Triples
{
    public abstract partial class TriplesObject
    {
        private async ValueTask SetPropertyDatabaseAsync(string name, object value, TriplesTransaction transaction)
        {
            var oldValue = await DoGetPropertyAsync(transaction, name);
            await DoSetPropertyAsync(transaction, name, value, oldValue);
        }

        private async ValueTask<object?> GetPropertyDatabaseAsync(string name, TriplesTransaction transaction)
        {
            return await DoGetPropertyAsync(transaction, name);
        }

        private async ValueTask DeletePropertyDatabaseAsync(string name, TriplesTransaction transaction)
        {
            var value = await DoGetPropertyAsync(transaction, name);
            await DoDeletePropertyAsync(transaction, name, value);
        }

        private async ValueTask<IEnumerable<KeyValuePair<string, object>>> GetAllPropertiesDatabaseAsync(TriplesTransaction transaction)
        {
            return await DoGetPropertiesAsync(transaction);
        }

        private async ValueTask DeleteAllPropertiesDatabaseAsync(TriplesTransaction transaction)
        {
            await DoDeleteAllPropertiesAsync(transaction);
        }

        private async ValueTask DoSetPropertyAsync(TriplesTransaction transaction, string name, object value, object? oldValue)
        {
            if (oldValue is TriplesObject oldObject)
            {
                await oldObject.DoReleaseAsync(transaction);
            }

            if (value is TriplesObject triplesObject)
            {
                if (triplesObject.Status == ObjectStatus.Pending)
                {
                    var triplesObjectProperties = await triplesObject.GetAllPropertiesAsync();

                    await triplesObject.DoIdentifyAsync(transaction, Database!);

                    await transaction.InsertOrReplaceAsync(this, name, value);

                    foreach (var item in triplesObjectProperties)
                    {
                        await triplesObject.DoSetPropertyAsync(transaction, item.Key, item.Value, null);
                    }

                    await triplesObject.DoManageAsync(transaction);

                    await triplesObject.DoCreateAsync(transaction);
                }
                else
                {
                    throw new InvalidOperationException("Value object status should be pending.");
                }
            }
            else
            {
                await transaction.InsertOrReplaceAsync(this, name, value);
            }
        }

        private async ValueTask<object?> DoGetPropertyAsync(TriplesTransaction transaction, string name)
        {
            return await transaction.SelectAsync(this, name);
        }

        private async ValueTask DoDeletePropertyAsync(TriplesTransaction transaction, string name, object? value)
        {
            if (value is TriplesObject triplesObject)
            {
                await triplesObject.DoReleaseAsync(transaction);
            }

            await transaction.DeleteAsync(this, name);
        }

        private async ValueTask DoDeleteAllPropertiesAsync(TriplesTransaction transaction)
        {
            var properties = await DoGetPropertiesAsync(transaction);

            foreach (var item in properties)
            {
                var value = item.Value;

                if (value is TriplesObject triplesObject)
                {
                    await triplesObject.DoReleaseAsync(transaction);
                }
            }

            await transaction.DeleteAsync(this);
        }

        private async ValueTask<IEnumerable<KeyValuePair<string, object>>> DoGetPropertiesAsync(TriplesTransaction transaction)
        {
            return await transaction.SelectAsync(this);
        }

        private async ValueTask DoIdentifyAsync(TriplesTransaction transaction, TriplesDatabase database)
        {
            if (Id != null)
            {
                throw new InvalidOperationException("Cannot identify an object twice.");
            }

            var tempId = Id;
            var tempDatabase = database;

            var newId = this is TriplesRoot ? 0 : await transaction.GetNewIdentifierAsync();
            transaction.RunSideEffect(
                () =>
                {
                    Id = newId;
                    Database = database;
                },
                () =>
                {
                    Id = tempId;
                    Database = tempDatabase;
                });
        }

#pragma warning disable 1998
        private async ValueTask DoManageAsync(TriplesTransaction transaction)
#pragma warning restore 1998
        {
            var tempStatus = Status;

            transaction.RunSideEffect(
                () =>
                {
                    Status = ObjectStatus.Managed;
                },
                () =>
                {
                    Status = tempStatus;
                });
        }

        private async ValueTask DoCreateAsync(TriplesTransaction transaction)
        {
            try
            {
                await CreateAsync(transaction);
            }
            catch (Exception e)
            {
                Console.WriteLine("Run Create method failed:" + e);
                throw;
            }
        }

        private async ValueTask DoReleaseAsync(TriplesTransaction transaction)
        {
            try
            {
                await ReleaseAsync(transaction);
            }
            catch (Exception e)
            {
                Console.WriteLine("Run Release method failed:" + e);
                throw;
            }

            await DoDeleteAllPropertiesAsync(transaction);

            var tempDatabase = Database;
            var tempStatus = Status;
            transaction.RunSideEffect(
                () =>
                {
                    Database = null;
                    Status = ObjectStatus.Released;
                },
                () =>
                {
                    Database = tempDatabase;
                    Status = tempStatus;
                });
        }
    }
}
