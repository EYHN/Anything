using System;
using Microsoft.Data.Sqlite;

namespace Anything.Database.Provider
{
    public class SharedMemoryConnectionProvider : ISqliteConnectionProvider
    {
        private readonly string _connectionString;

        public SharedMemoryConnectionProvider(string name)
        {
            Name = name;
            _connectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory, DataSource = name, Cache = SqliteCacheMode.Shared, RecursiveTriggers = true
            }.ToString();
        }

        public string Name { get; }

        public SqliteConnection Make(SqliteOpenMode mode, bool isolated)
        {
            var connection = new SqliteConnection(_connectionString);
            var initializeCommand = connection.CreateCommand();
            initializeCommand.CommandText = @"
            PRAGMA case_sensitive_like=true;
            ";
            try
            {
                connection.Open();
                initializeCommand.ExecuteNonQuery();
                connection.Close();
                return connection;
            }
            catch (SqliteException sqliteException)
            {
                throw new AggregateException("Can't create sqlite connection in memory.", sqliteException);
            }
        }
    }
}
