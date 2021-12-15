using System;
using System.Data;

namespace Anything.Database.Extensions;

public static class IDataReaderExtensions
{
    public static string? GetNullableString(this IDataReader dataReader, int ordinal)
    {
        return dataReader.IsDBNull(ordinal) ? null : dataReader.GetString(ordinal);
    }

    public static long? GetNullableInt64(this IDataReader dataReader, int ordinal)
    {
        return dataReader.IsDBNull(ordinal) ? null : dataReader.GetInt64(ordinal);
    }

    public static int? GetNullableInt32(this IDataReader dataReader, int ordinal)
    {
        return dataReader.IsDBNull(ordinal) ? null : dataReader.GetInt32(ordinal);
    }

    public static DateTimeOffset? GetNullableDateTimeOffsetFromUnixTimeMilliseconds(this IDataReader dataReader, int ordinal)
    {
        return dataReader.IsDBNull(ordinal) ? null : DateTimeOffset.FromUnixTimeMilliseconds(dataReader.GetInt64(ordinal));
    }

    public static T GetEnum<T>(this IDataReader dataReader, int ordinal)
        where T : Enum
    {
        return (T)Enum.ToObject(typeof(T), dataReader.GetInt32(ordinal));
    }

    public static T? GetNullableEnum<T>(this IDataReader dataReader, int ordinal)
        where T : Enum
    {
        if (!dataReader.IsDBNull(ordinal))
        {
            return (T)Enum.ToObject(typeof(T), dataReader.GetInt32(ordinal));
        }

        return default;
    }
}
