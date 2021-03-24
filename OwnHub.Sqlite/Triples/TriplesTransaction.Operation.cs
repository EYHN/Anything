using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Triples
{
    public sealed partial class TriplesTransaction
    {
        public async ValueTask InsertAsync(TriplesObject subject, string predicate, object obj)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            Database.Serializer.Serialize(obj, out var objectSerialized, out var objectTypeDesc);

            await Database.TriplesTable.InsertAsync(
                this,
                subject.Id!.Value,
                predicate,
                objectSerialized,
                objectTypeDesc.ToTypeDescText());
        }

        public void Insert(TriplesObject subject, string predicate, object obj)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            Database.Serializer.Serialize(obj, out var objectSerialized, out var objectTypeDesc);

            Database.TriplesTable.Insert(
                this,
                subject.Id!.Value,
                predicate,
                objectSerialized,
                objectTypeDesc.ToTypeDescText());
        }

        public async ValueTask InsertOrReplaceAsync(TriplesObject subject, string predicate, object obj)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            Database.Serializer.Serialize(obj, out var objectSerialized, out var objectTypeDesc);

            await Database.TriplesTable.InsertOrReplaceAsync(
                this,
                subject.Id!.Value,
                predicate,
                objectSerialized,
                objectTypeDesc.ToTypeDescText());
        }

        public void InsertOrReplace(TriplesObject subject, string predicate, object obj)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            Database.Serializer.Serialize(obj, out var objectSerialized, out var objectTypeDesc);

            Database.TriplesTable.InsertOrReplace(
                this,
                subject.Id!.Value,
                predicate,
                objectSerialized,
                objectTypeDesc.ToTypeDescText());
        }

        public async ValueTask DeleteAsync(TriplesObject subject, string predicate)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            await Database.TriplesTable.DeleteAsync(this, subject.Id!.Value, predicate);
        }

        public void Delete(TriplesObject subject, string predicate)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            Database.TriplesTable.Delete(this, subject.Id!.Value, predicate);
        }

        public async ValueTask DeleteAsync(TriplesObject subject)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            await Database.TriplesTable.DeleteAsync(this, subject.Id!.Value, null);
        }

        public void Delete(TriplesObject subject)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            Database.TriplesTable.Delete(this, subject.Id!.Value, null);
        }

        public async ValueTask<object?> SelectAsync(TriplesObject subject, string predicate)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            return await Database.TriplesTable.SelectAsync(this, subject.Id!.Value, predicate, ReadSingleValue);
        }

        public object? Select(TriplesObject subject, string predicate)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            return Database.TriplesTable.Select(this, subject.Id!.Value, predicate, ReadSingleValue);
        }

        public async ValueTask<IEnumerable<KeyValuePair<string, object>>> SelectAsync(TriplesObject subject)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            return await Database.TriplesTable.SelectAsync(this, subject.Id!.Value, null, ReadKeyValuePairs);
        }

        public IEnumerable<KeyValuePair<string, object>> Select(TriplesObject subject)
        {
            if (subject.Id == null)
            {
                throw new ArgumentException("Subject has no id.");
            }

            return Database.TriplesTable.Select(this, subject.Id!.Value, null, ReadKeyValuePairs);
        }

        public async ValueTask<long> GetNewIdentifierAsync()
        {
            return await Database.TriplesSequenceTable.IncreaseSeqAsync(DbConnection, TriplesDatabase.SequenceName);
        }

        public long GetNewIdentifier()
        {
            return Database.TriplesSequenceTable.IncreaseSeq(DbConnection, TriplesDatabase.SequenceName);
        }

        private object? ReadSingleValue(SqliteDataReader reader)
        {
            if (!reader.Read())
            {
                return null;
            }

            var objectType = reader.GetString(reader.GetOrdinal("ObjectType"));

            var obj =
                Database.Serializer.DeserializeFromSqliteDataReader(reader, reader.GetOrdinal("Object"), TriplesTypeDesc.ParseTypeDescText(objectType), Database);
#if DEBUG
            Debug.Assert(reader.Read() == false, "The reader should be ended.");
#endif
            return obj;
        }

        private IEnumerable<KeyValuePair<string, object>> ReadKeyValuePairs(SqliteDataReader reader)
        {
            List<KeyValuePair<string, object>> result = new();

            while (reader.Read())
            {
                var predicate = reader.GetString(reader.GetOrdinal("Predicate"));
                var objectType = reader.GetString(reader.GetOrdinal("ObjectType"));
                var obj =
                    Database.Serializer.DeserializeFromSqliteDataReader(reader, reader.GetOrdinal("Object"), TriplesTypeDesc.ParseTypeDescText(objectType), Database);

                result.Add(new KeyValuePair<string, object>(predicate, obj));
            }

            return result;
        }
    }
}
