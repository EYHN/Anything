using System;
using Microsoft.Data.Sqlite;

namespace Anything.Database.Provider;

public class SqliteConnectionProvider : ISqliteConnectionProvider
{
    private readonly string _databaseFile;

    public SqliteConnectionProvider(string databaseFile)
    {
        _databaseFile = databaseFile;
    }

    public SqliteConnection Make(SqliteOpenMode mode, bool isolated = false)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            Mode = mode,
            DataSource = _databaseFile,
            RecursiveTriggers = true,
            Cache = !isolated ? SqliteCacheMode.Shared : SqliteCacheMode.Private
        }.ToString();
        var connection = new SqliteConnection(connectionString);
        using var initializeCommand = connection.CreateCommand();
        initializeCommand.CommandText = @"
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            PRAGMA cache_size=4000;
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
            throw new AggregateException($"Can't create sqlite connection for database file '{_databaseFile}'", sqliteException);
        }
    }
}
