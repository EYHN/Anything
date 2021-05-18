using System;
using System.Collections.Generic;
using System.Data.Common;
using StagingBox.Database.Orm;
using StagingBox.Database.Table;

namespace StagingBox.Database.Triples
{
    /// <summary>
    /// Provides methods to access the triples database.
    /// </summary>
    public class TriplesDatabaseProvider : OrmDatabaseProvider
    {
        internal SqliteContext Context { get; }

        internal TriplesTable TriplesTable { get; }

        internal SequenceTable SequenceTable { get; }

        private const string SequenceName = "Object";

        public TriplesDatabaseProvider(SqliteContext context, string tableName)
        {
            Context = context;
            TriplesTable = new TriplesTable(tableName);
            SequenceTable = new SequenceTable(tableName + "Sequence");
        }

        /// <inheritdoc/>
        public override void Create(OrmTransaction transaction)
        {
            TriplesTable.Create(transaction);
            SequenceTable.Create(transaction);
            SequenceTable.Insert(transaction, SequenceName, ignoreIfExist: true);
        }

        /// <inheritdoc/>
        public override bool TryReadObject(OrmTransaction transaction, long objectId, OrmTypeInfo typeInfo, out OrmSnapshot snapshot)
        {
            snapshot = new OrmSnapshot();
            foreach (var property in typeInfo.Properties)
            {
                var propertyName = property.Name;
                var propertyIsLazy = property.IsLazy;
                var value = ReadProperty(transaction, objectId, propertyName, out _, propertyIsLazy);

                if (value == null)
                {
                    snapshot.Add(property, null);
                    continue;
                }

                snapshot.Add(property, value);
            }

            return true;
        }

        /// <inheritdoc/>
        public override void Update(
            OrmTransaction transaction,
            long objectId,
            OrmTypeInfo typeInfo,
            IEnumerable<OrmSnapshot.DiffResult> changing)
        {
            foreach (var pair in changing)
            {
                var property = pair.PropertyInfo;
                var propertyIsLazy = property.IsLazy;

                var unlazyValue = propertyIsLazy && pair.New != null ? OrmLazy.GetValue(pair.New) : pair.New;

                InsertOrReplaceProperty(transaction, objectId, property.Name, unlazyValue, property.ValueTypeInfo);
            }
        }

        /// <inheritdoc/>
        public override void Insert(OrmTransaction transaction, long objectId, OrmTypeInfo typeInfo, OrmSnapshot snapshot)
        {
            foreach (var pair in snapshot)
            {
                var property = pair.Key;
                var propertyIsLazy = property.IsLazy;
                var value = propertyIsLazy && pair.Value != null ? OrmLazy.GetValue(pair.Value) : pair.Value;

                if (value == null)
                {
                    continue;
                }

                InsertProperty(transaction, objectId, property.Name, value, property.ValueTypeInfo);
            }
        }

        /// <inheritdoc/>
        public override void Release(OrmTransaction transaction, long objectId)
        {
            var values = TriplesTable.Select(
                transaction,
                objectId,
                null,
                HandleReaderKeyValuePairs);

            foreach (var pair in values)
            {
                var predicate = pair.Key;
                var parsedValue = ParseReaderResult(transaction, pair.Value, out var propertyTypeInfo, out var propertyObjectId, true);

                if (parsedValue != null && propertyTypeInfo != null && !propertyTypeInfo.IsScalar)
                {
                    // is a object
                    transaction.Release(OrmLazy.GetValue(parsedValue)!);
                    TriplesTable.Delete(transaction, objectId, predicate);
                }
                else if (propertyObjectId != null)
                {
                    // is a object, but unknown type
                    Release(transaction, propertyObjectId.Value);
                    TriplesTable.Delete(transaction, objectId, predicate);
                }
                else
                {
                    TriplesTable.Delete(transaction, objectId, predicate);
                }
            }
        }

        /// <inheritdoc />
        public override IDbTransaction StartTransaction(ITransaction.TransactionMode mode)
        {
            return new SqliteTransaction(Context, mode);
        }

        /// <inheritdoc/>
        public override long NextObjectId(OrmTransaction transaction, OrmTypeInfo typeInfo)
        {
            return typeInfo.TargetType == typeof(TriplesRoot) ? 0 : SequenceTable.IncreaseSeq(transaction, SequenceName);
        }

        public void InsertProperty(
            OrmTransaction transaction,
            long objectId,
            string predicate,
            object value,
            OrmTypeInfo? typeInfo = null,
            bool allowReplace = false)
        {
            var newValueTypeInfo = typeInfo ?? OrmSystem.TypeManager.GetOrmTypeInfo(value.GetType());
            Action<OrmTransaction, long, string, object, string> insertMethod =
                allowReplace ? TriplesTable.InsertOrReplace : TriplesTable.Insert;

            if (newValueTypeInfo.IsScalar)
            {
                if (TriplesValueUtils.TryGetValueType(newValueTypeInfo.TargetType, out var valueType))
                {
                    var typeDesc = new TriplesTypeDesc(TriplesTypeCategory.Value, valueType.ToString());
                    insertMethod(transaction, objectId, predicate, value, typeDesc.ToTypeDescText());
                }
                else
                {
                    throw new InvalidOperationException("Not allowed type.");
                }
            }
            else
            {
                var newObjectId = transaction.NextObjectId(newValueTypeInfo);
                insertMethod(
                    transaction,
                    objectId,
                    predicate,
                    newObjectId,
                    new TriplesTypeDesc(TriplesTypeCategory.Object, newValueTypeInfo.Name).ToTypeDescText());

                transaction.Insert(value, newObjectId);
            }
        }

        public void InsertOrReplaceProperty(
            OrmTransaction transaction,
            long objectId,
            string predicate,
            object? value,
            OrmTypeInfo? typeInfo = null)
        {
            var oldValue = ReadProperty(transaction, objectId, predicate, out var oldValueTypeInfo);

            if (oldValueTypeInfo != null && oldValue != null && !oldValueTypeInfo.IsScalar)
            {
                transaction.Release(oldValue);
            }

            if (value == null)
            {
                TriplesTable.Delete(transaction, objectId, predicate);
                return;
            }

            InsertProperty(transaction, objectId, predicate, value, typeInfo, true);
        }

        public object? ReadProperty(
            OrmTransaction transaction,
            long objectId,
            string predicate,
            out OrmTypeInfo? outTypeInfo,
            bool lazy = false)
        {
            var result = TriplesTable.Select(
                transaction,
                objectId,
                predicate,
                HandleReaderSingleValue);

            if (result == null)
            {
                outTypeInfo = null;
                return null;
            }

            return ParseReaderResult(transaction, result, out outTypeInfo, out _, lazy);
        }

        private object? ParseReaderResult(
            OrmTransaction transaction,
            ReaderResultItem result,
            out OrmTypeInfo? outTypeInfo,
            out long? outObjectId,
            bool lazy = false)
        {
            var typeName = result.TypeDesc.Name;
            var typeIsObject = result.TypeDesc.IsObject;
            if (typeName == null)
            {
                throw new InvalidOperationException("Error type name.");
            }

            if (typeIsObject)
            {
                var valueObjectId = (long)result.Value;
                if (OrmSystem.TypeManager.TryGetOrmTypeInfo(typeName, out var typeInfo))
                {
                    if (lazy)
                    {
                        outTypeInfo = typeInfo;
                        outObjectId = valueObjectId;
                        return transaction.CreateLazyObject(valueObjectId, typeInfo);
                    }

                    if (transaction.TryGetObject(valueObjectId, typeInfo, out var value))
                    {
                        outTypeInfo = typeInfo;
                        outObjectId = valueObjectId;
                        return value;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Can't read object, objectid: {valueObjectId}");
                    }
                }
                else
                {
                    // is unknown type
                    outTypeInfo = null;
                    outObjectId = valueObjectId;
                    return null;
                }
            }
            else
            {
                // is scalar
                if (TriplesValueUtils.TryGetType(typeName, out var resultType))
                {
                    if (OrmSystem.TypeManager.TryGetOrmTypeInfo(resultType, out var valueTypeInfo))
                    {
                        outTypeInfo = valueTypeInfo;
                        outObjectId = null;
                        return result.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid state.");
                    }
                }
                else
                {
                    // is unknown type
                    outTypeInfo = null;
                    outObjectId = null;
                    return null;
                }
            }
        }

        private ReaderResultItem? HandleReaderSingleValue(DbDataReader reader)
        {
            if (!reader.Read())
            {
                return null;
            }

            var objectType = reader.GetString(reader.GetOrdinal("ObjectType"));
            var typeDesc = TriplesTypeDesc.ParseTypeDescText(objectType);
            var isScalar = typeDesc.Category == TriplesTypeCategory.Value;

            if (!isScalar)
            {
                return new ReaderResultItem { TypeDesc = typeDesc, Value = reader.GetInt64(reader.GetOrdinal("Object")) };
            }

            var valueType = Enum.Parse<TriplesValueType>(typeDesc.Name!);
            return new ReaderResultItem
            {
                TypeDesc = typeDesc,
                Value = TriplesValueUtils.GetValueFromSqliteDataReader(reader, reader.GetOrdinal("Object"), valueType)
            };
        }

        private IEnumerable<KeyValuePair<string, ReaderResultItem>> HandleReaderKeyValuePairs(DbDataReader reader)
        {
            List<KeyValuePair<string, ReaderResultItem>> result = new();

            while (reader.Read())
            {
                var predicate = reader.GetString(reader.GetOrdinal("Predicate"));
                var objectType = reader.GetString(reader.GetOrdinal("ObjectType"));
                var typeDesc = TriplesTypeDesc.ParseTypeDescText(objectType);
                var isScalar = typeDesc.Category == TriplesTypeCategory.Value;

                if (!isScalar)
                {
                    result.Add(new KeyValuePair<string, ReaderResultItem>(
                        predicate,
                        new ReaderResultItem { TypeDesc = typeDesc, Value = reader.GetInt64(reader.GetOrdinal("Object")) }));

                    continue;
                }

                var valueType = Enum.Parse<TriplesValueType>(typeDesc.Name!);
                result.Add(new KeyValuePair<string, ReaderResultItem>(
                    predicate,
                    new ReaderResultItem
                    {
                        TypeDesc = typeDesc,
                        Value = TriplesValueUtils.GetValueFromSqliteDataReader(reader, reader.GetOrdinal("Object"), valueType)
                    }));
            }

            return result;
        }

        private record ReaderResultItem
        {
            public TriplesTypeDesc TypeDesc { get; init; }

            public object Value { get; init; }
        }
    }
}
