using System;
using Microsoft.Data.Sqlite;

namespace Anything.Database.Provider
{
    public class SharedMemoryConnectionProvider : ISqliteConnectionProvider
    {
        public string ConnectionString { get; }

        public string Name { get; }

        public SharedMemoryConnectionProvider(string name)
        {
            Name = name;
            ConnectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory, DataSource = name, Cache = SqliteCacheMode.Shared, RecursiveTriggers = true
            }.ToString();
        }

        public SqliteConnection Make(SqliteOpenMode mode, bool isolated)
        {
            var connection = new SqliteConnection(ConnectionString);
            using var initializeCommand = connection.CreateCommand();
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
                connection.Dispose();
                throw new AggregateException("Can't create sqlite connection in memory.", sqliteException);
            }
        }
    }
}
