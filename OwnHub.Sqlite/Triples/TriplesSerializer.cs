using System;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Triples
{
    internal sealed class TriplesSerializer
    {
        private readonly TriplesObjectTypeRegistry _objectTypeRegistry;

        public TriplesSerializer(TriplesObjectTypeRegistry objectTypeRegistry)
        {
            _objectTypeRegistry = objectTypeRegistry;
        }

        public void Serialize(object obj, out object data, out TriplesTypeDesc typeDesc)
        {
            var type = obj.GetType();

            if (obj is TriplesObject triplesObject)
            {
                if (triplesObject.Id == null)
                {
                    throw new ArgumentException("The object not have id.");
                }

                if (_objectTypeRegistry.TryGetTypeDesc(type, out var registeredTypeDesc))
                {
                    typeDesc = registeredTypeDesc;

                    data = triplesObject.Id ?? throw new ArgumentException("Identifier is null.");
                }
                else
                {
                    throw new ArgumentException("Unregistered triples object type:" + type.Name);
                }
            }
            else
            {
                if (TriplesValueUtils.TryGetValueType(type, out var valueType))
                {
                    typeDesc = new TriplesTypeDesc(TriplesTypeCategory.Value, valueType.ToString());
                    data = obj;
                }
                else
                {
                    throw new ArgumentException("Unknown type:" + type.Name);
                }
            }
        }

        public object DeserializeFromSqliteDataReader(SqliteDataReader reader, int ordinal, TriplesTypeDesc typeDesc, TriplesDatabase database)
        {
            if (typeDesc.IsObject)
            {
                if (_objectTypeRegistry.TryGetType(typeDesc, out var type))
                {
                    var data = reader.GetInt64(ordinal);
                    return TriplesObject.RestoreFromDatabase(type, database, data);
                }
                else
                {
                    throw new ArgumentException("Unregistered triples object type:" + typeDesc.ToTypeDescText());
                }
            }
            else
            {
                var valueType = Enum.Parse<TriplesValueType>(typeDesc.Name!);
                return TriplesValueUtils.GetValueFromSqliteDataReader(reader, ordinal, valueType);
            }
        }
    }
}
