using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace Anything.Database.Triples
{
    internal enum TriplesValueType
    {
#pragma warning disable SA1602
        Bool = 0,
        Byte = 1,
        Blob = 2,
        Char = 3,
        DateTime = 4,
        DateTimeOffset = 5,
        Decimal = 6,
        Double = 7,
        Float = 8,
        Guid = 9,
        Int = 10,
        Long = 11,
        Sbyte = 12,
        Short = 13,
        String = 14,
        TimeSpan = 15,
        Uint = 16,
        Ulong = 17,
        Ushort = 18
#pragma warning restore SA1012
    }

    internal static class TriplesValueUtils
    {
        private static readonly Dictionary<Type, TriplesValueType> _valueTypeMapping =
            new()
            {
                { typeof(bool), TriplesValueType.Bool },
                { typeof(byte), TriplesValueType.Byte },
                { typeof(byte[]), TriplesValueType.Blob },
                { typeof(char), TriplesValueType.Char },
                { typeof(DateTime), TriplesValueType.DateTime },
                { typeof(DateTimeOffset), TriplesValueType.DateTimeOffset },
                { typeof(decimal), TriplesValueType.Decimal },
                { typeof(double), TriplesValueType.Double },
                { typeof(float), TriplesValueType.Float },
                { typeof(Guid), TriplesValueType.Guid },
                { typeof(int), TriplesValueType.Int },
                { typeof(long), TriplesValueType.Long },
                { typeof(sbyte), TriplesValueType.Sbyte },
                { typeof(short), TriplesValueType.Short },
                { typeof(string), TriplesValueType.String },
                { typeof(TimeSpan), TriplesValueType.TimeSpan },
                { typeof(uint), TriplesValueType.Uint },
                { typeof(ulong), TriplesValueType.Ulong },
                { typeof(ushort), TriplesValueType.Ushort }
            };

        public static bool TryGetValueType(object value, [MaybeNullWhen(false)] out TriplesValueType valueType)
        {
            var type = value.GetType();
            return _valueTypeMapping.TryGetValue(type, out valueType);
        }

        public static bool TryGetValueType(Type type, [MaybeNullWhen(false)] out TriplesValueType valueType)
        {
            return _valueTypeMapping.TryGetValue(type, out valueType);
        }

        public static bool TryGetType(TriplesValueType valueType, [MaybeNullWhen(false)] out Type type)
        {
            foreach (var pair in _valueTypeMapping)
            {
                if (pair.Value == valueType)
                {
                    type = pair.Key;
                    return true;
                }
            }

            type = null;
            return false;
        }

        public static bool TryGetType(string valueTypeName, [MaybeNullWhen(false)] out Type type)
        {
            return TryGetType(Enum.Parse<TriplesValueType>(valueTypeName), out type);
        }

        public static object GetValueFromSqliteDataReader(DbDataReader reader, int ordinal, TriplesValueType valueType)
        {
            switch (valueType)
            {
                case TriplesValueType.Bool:
                    return reader.GetBoolean(ordinal);
                case TriplesValueType.Byte:
                    return reader.GetByte(ordinal);
                case TriplesValueType.Blob:
                    using (var memoryStream = new MemoryStream())
                    {
                        reader.GetStream(ordinal).CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }

                case TriplesValueType.Char:
                    return reader.GetChar(ordinal);
                case TriplesValueType.DateTime:
                    return reader.GetDateTime(ordinal);
                case TriplesValueType.DateTimeOffset:
                    return (reader as SqliteDataReader)!.GetDateTimeOffset(ordinal);
                case TriplesValueType.Decimal:
                    return reader.GetDecimal(ordinal);
                case TriplesValueType.Double:
                    return reader.GetDouble(ordinal);
                case TriplesValueType.Float:
                    return reader.GetFloat(ordinal);
                case TriplesValueType.Guid:
                    return reader.GetGuid(ordinal);
                case TriplesValueType.Int:
                    return reader.GetInt32(ordinal);
                case TriplesValueType.Long:
                    return reader.GetInt64(ordinal);
                case TriplesValueType.Sbyte:
                    return checked((sbyte)reader.GetInt64(ordinal));
                case TriplesValueType.Short:
                    return reader.GetInt16(ordinal);
                case TriplesValueType.String:
                    return reader.GetString(ordinal);
                case TriplesValueType.TimeSpan:
                    return (reader as SqliteDataReader)!.GetTimeSpan(ordinal);
                case TriplesValueType.Uint:
                    return checked((uint)reader.GetInt64(ordinal));
                case TriplesValueType.Ulong:
                    return (ulong)reader.GetInt64(ordinal);
                case TriplesValueType.Ushort:
                    return checked((ushort)reader.GetInt64(ordinal));
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "Unknown TriplesValueType");
            }
        }
    }
}
