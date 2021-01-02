using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Table
{
    /// <summary>
    /// Collection of triples, each consisting of a subject, a predicate and an object.
    /// Each triple represents a statement of a relationship between the things denoted by the nodes that it links. Each triple has three parts:
    /// 1. a subject
    /// 2. an object, and
    /// 3. a predicate (also called a property) that denotes a relationship.
    /// 
    /// The direction of the arc is significant: it always points toward the object.
    ///
    ///            Predicate
    /// Subject --------------> Object
    /// </summary>
    public class TriplesTable
    {
        private string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {tableName};
            ";

        private string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {tableName} (
                id INTEGER PRIMARY KEY,
                subject TEXT NOT NULL,
                predicate TEXT NOT NULL,
                object,
                type TEXT
            );
            CREATE UNIQUE INDEX IF NOT EXISTS {tableName}SubjectPredicateIndex ON {tableName} (subject, predicate);
            CREATE INDEX IF NOT EXISTS {tableName}SubjectIndex ON {tableName} (subject);
            CREATE INDEX IF NOT EXISTS {tableName}PredicateIndex ON {tableName} (predicate);
            ";
        
        private readonly SqliteContext context;
        private readonly string tableName;
        
        public TriplesTable(SqliteContext context, string tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }
        
        public Task Create()
        {
            return context.Create(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = DatabaseCreateCommand;
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public Task Drop()
        {
            return context.Write(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = DatabaseDropCommand;
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public Task Insert(string subject, string predicate, bool obj) => Insert(subject, predicate, obj, "bool");
        public Task Insert(string subject, string predicate, byte obj) => Insert(subject, predicate, obj, "byte");
        public Task Insert(string subject, string predicate, char obj) => Insert(subject, predicate, obj, "char");
        public Task Insert(string subject, string predicate, DateTime obj) => Insert(subject, predicate, obj, "DateTime");
        public Task Insert(string subject, string predicate, DateTimeOffset obj) => Insert(subject, predicate, obj, "DateTimeOffset");
        public Task Insert(string subject, string predicate, Decimal obj) => Insert(subject, predicate, obj, "Decimal");
        public Task Insert(string subject, string predicate, double obj) => Insert(subject, predicate, obj, "double");
        public Task Insert(string subject, string predicate, float obj) => Insert(subject, predicate, obj, "float");
        public Task Insert(string subject, string predicate, Guid obj) => Insert(subject, predicate, obj, "Guid");
        public Task Insert(string subject, string predicate, int obj) => Insert(subject, predicate, obj, "int");
        public Task Insert(string subject, string predicate, long obj) => Insert(subject, predicate, obj, "long");
        public Task Insert(string subject, string predicate, sbyte obj) => Insert(subject, predicate, obj, "sbyte");
        public Task Insert(string subject, string predicate, short obj) => Insert(subject, predicate, obj, "short");
        public Task Insert(string subject, string predicate, string obj) => Insert(subject, predicate, obj, "string");
        public Task Insert(string subject, string predicate, TimeSpan obj) => Insert(subject, predicate, obj, "TimeSpan");
        public Task Insert(string subject, string predicate, uint obj) => Insert(subject, predicate, obj, "uint");
        public Task Insert(string subject, string predicate, ulong obj) => Insert(subject, predicate, obj, "ulong");
        public Task Insert(string subject, string predicate, ushort obj) => Insert(subject, predicate, obj, "ushort");
        public Task Insert(string subject, string predicate, object? obj) => Insert(subject, predicate, obj, "object");
        
        private Task Insert(string subject, string predicate, object? obj, string type)
        {
            return context.Write(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                INSERT INTO {tableName} (subject, predicate{(obj != null ? ", object" : "")}, type) VALUES(
                    $subject, $predicate{(obj != null ? ", $object" : "")}, $type
                );
                ";;
                command.Parameters.AddWithValue("$subject", subject);
                command.Parameters.AddWithValue("$predicate", predicate);
                if (obj != null) command.Parameters.AddWithValue("$object", obj);
                command.Parameters.AddWithValue("$type", type);
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public Task<Row?> Search(
            long id
        )
        {
            return context.Read(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                SELECT id, subject, predicate, object, type FROM {tableName}
                    WHERE id=$id
                ";
                command.Parameters.AddWithValue("$id", id);
                SqliteDataReader reader = await command.ExecuteReaderAsync();
                
                if (reader.Read())
                {
                    string type = reader.GetString(4);
                    object? obj = reader.GetFieldValue(3, type);
                    return new Row()
                    {
                        Id = reader.GetInt64(0),
                        Subject = reader.GetString(1),
                        Predicate = reader.GetString(2),
                        Obj = obj,
                        Type = type
                    };
                }

                return null;
            });
        }
        
        public Task<Row?> Search(
            string subject, string predicate
        )
        {
            return context.Read(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                SELECT id, subject, predicate, object, type FROM {tableName}
                    WHERE subject=$subject AND predicate=$predicate
                ";
                command.Parameters.AddWithValue("$subject", subject);
                command.Parameters.AddWithValue("$predicate", predicate);
                SqliteDataReader reader = await command.ExecuteReaderAsync();
                
                if (reader.Read())
                {
                    string type = reader.GetString(4);
                    object? obj = reader.GetFieldValue(3, type);
                    return new Row()
                    {
                        Id = reader.GetInt64(0),
                        Subject = reader.GetString(1),
                        Predicate = reader.GetString(2),
                        Obj = obj,
                        Type = type
                    };
                }

                return null;
            });
        }

        public Task Delete(long id)
        {
            return context.Read(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"DELETE FROM {tableName} WHERE id=$id";;
                command.Parameters.AddWithValue("$id", id);
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public Task Delete(string subject, string predicate)
        {
            return context.Read(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"DELETE FROM {tableName} WHERE subject=$subject AND predicate=$predicate";;
                command.Parameters.AddWithValue("$subject", subject);
                command.Parameters.AddWithValue("$predicate", predicate);
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public class Row
        {
            public long Id;
            public string Subject = null!;
            public string Predicate = null!;
            public object? Obj;
            public string Type;
        }
    }
    
    internal static class TriplesTableSqliteDataReaderExtensions
    {
        public static object? GetFieldValue(this SqliteDataReader reader, int ordinal, string type)
        {
            if (type == "null" || reader.IsDBNull(ordinal))
                return null;
            if (type == "bool")
                return reader.GetBoolean(ordinal);
            if (type == "byte")
                return reader.GetByte(ordinal);
            if (type == "char")
                return reader.GetChar(ordinal);
            if (type == "DateTime")
                return reader.GetDateTime(ordinal);
            if (type == "DateTimeOffset")
                return reader.GetDateTimeOffset(ordinal);
            if (type == "Decimal")
                return reader.GetDecimal(ordinal);
            if (type == "double")
                return reader.GetDouble(ordinal);
            if (type == "float")
                return reader.GetFloat(ordinal);
            if (type == "Guid")
                return reader.GetGuid(ordinal);
            if (type == "int")
                return reader.GetInt32(ordinal);
            if (type == "long")
                return reader.GetInt64(ordinal);
            if (type == "sbyte")
                return checked ((sbyte) reader.GetInt64(ordinal));
            if (type == "short")
                return reader.GetInt16(ordinal);
            if (type == "string")
                return reader.GetString(ordinal);
            if (type == "TimeSpan")
                return reader.GetTimeSpan(ordinal);
            if (type == "uint")
                return checked ((uint) reader.GetInt64(ordinal));
            if (type == "ulong")
                return (ulong) reader.GetInt64(ordinal);
            if (type == "ushort")
                return checked ((ushort) reader.GetInt64(ordinal));
            return reader.GetValue(ordinal);
        }
    }
}