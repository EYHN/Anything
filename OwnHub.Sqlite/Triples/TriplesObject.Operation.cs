using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OwnHub.Sqlite.Triples
{
    public abstract partial class TriplesObject
    {
        private void SetPropertyDatabase(string name, object value, TriplesTransaction transaction)
        {
            DoTryGetProperty(transaction, name, out var oldValue);
            DoSetProperty(transaction, name, value, oldValue);
        }

        private bool TryGetPropertyDatabase(string name, [MaybeNullWhen(false)] out object obj, TriplesTransaction transaction)
        {
            return DoTryGetProperty(transaction, name, out obj);
        }

        private void DeletePropertyDatabase(string name, TriplesTransaction transaction)
        {
            DoTryGetProperty(transaction, name, out var value);
            DoDeleteProperty(transaction, name, value);
        }

        private IEnumerable<KeyValuePair<string, object>> GetAllPropertiesDatabase(TriplesTransaction transaction)
        {
            return DoGetProperties(transaction);
        }

        private void DeleteAllPropertiesDatabase(TriplesTransaction transaction)
        {
            DoDeleteAllProperties(transaction);
        }

        private void DoSetProperty(TriplesTransaction transaction, string name, object value, object? oldValue)
        {
            if (oldValue is TriplesObject oldObject)
            {
                oldObject.DoRelease(transaction);
            }

            if (value is TriplesObject triplesObject)
            {
                if (triplesObject.Status == ObjectStatus.Pending)
                {
                    var triplesObjectProperties = triplesObject.GetAllProperties();

                    triplesObject.DoIdentify(transaction, Database!);

                    transaction.InsertOrReplace(this, name, triplesObject);

                    foreach (var item in triplesObjectProperties)
                    {
                        triplesObject.DoSetProperty(transaction, item.Key, item.Value, null);
                    }

                    triplesObject.DoManage(transaction);

                    triplesObject.DoCreate(transaction);
                }
                else
                {
                    throw new InvalidOperationException("Value object status should be pending.");
                }
            }
            else
            {
                transaction.InsertOrReplace(this, name, value);
            }
        }

        private bool DoTryGetProperty(TriplesTransaction transaction, string name, [MaybeNullWhen(false)] out object obj)
        {
            obj = transaction.Select(this, name);
            return obj != null;
        }

        private void DoDeleteProperty(TriplesTransaction transaction, string name, object? value)
        {
            if (value is TriplesObject triplesObject)
            {
                triplesObject.DoRelease(transaction);
            }

            transaction.Delete(this, name);
        }

        private void DoDeleteAllProperties(TriplesTransaction transaction)
        {
            var properties = DoGetProperties(transaction);

            foreach (var item in properties)
            {
                var value = item.Value;

                if (value is TriplesObject triplesObject)
                {
                    triplesObject.DoRelease(transaction);
                }
            }

            transaction.Delete(this);
        }

        private IEnumerable<KeyValuePair<string, object>> DoGetProperties(TriplesTransaction transaction)
        {
            return transaction.Select(this);
        }

        private void DoIdentify(TriplesTransaction transaction, TriplesDatabase database)
        {
            if (Id != null)
            {
                throw new InvalidOperationException("Cannot identify an object twice.");
            }

            var tempId = Id;
            var tempDatabase = database;

            var newId = this is TriplesRoot ? 0 : transaction.GetNewIdentifier();
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

        private void DoManage(TriplesTransaction transaction)
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

        private void DoCreate(TriplesTransaction transaction)
        {
            try
            {
                Create(transaction);
            }
            catch (Exception e)
            {
                Console.WriteLine("Run Create method failed:" + e);
                throw;
            }
        }

        private void DoRelease(TriplesTransaction transaction)
        {
            try
            {
                Release(transaction);
            }
            catch (Exception e)
            {
                Console.WriteLine("Run Release method failed:" + e);
                throw;
            }

            DoDeleteAllProperties(transaction);

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
