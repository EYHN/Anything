using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Utils;

namespace OwnHub.Sqlite
{
    public class SqliteTransaction : Transaction
    {
        private ObjectPool<SqliteConnection>.Ref? _dbConnectionRef;
        private Microsoft.Data.Sqlite.SqliteTransaction? _dbTransaction;
        private SqliteCommandCache _dbCommandCache = new();
        private bool _disposed;

        public SqliteConnection DbConnection
        {
            get
            {
                EnsureDbTransaction();
                return _dbConnectionRef!.Value;
            }
        }

        public Microsoft.Data.Sqlite.SqliteTransaction DbTransaction
        {
            get
            {
                EnsureDbTransaction();
                return _dbTransaction!;
            }
        }

        /// <summary>
        /// Gets the associated context of this transaction.
        /// </summary>
        public SqliteContext Context { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteTransaction"/> class.
        /// </summary>
        /// <param name="context">Associated context.</param>
        /// <param name="mode">Transaction mode.</param>
        public SqliteTransaction(
            SqliteContext context,
            TransactionMode mode)
            : base(mode)
        {
            Context = context;
        }

        private void EnsureDbTransaction()
        {
            if (_dbConnectionRef == null || _dbTransaction == null)
            {
                StartDbTransaction();
            }
        }

        private async ValueTask EnsureDbTransactionAsync()
        {
            if (_dbConnectionRef == null || _dbTransaction == null)
            {
                await StartDbTransactionAsync();
            }
        }

        private void StartDbTransaction()
        {
            _dbConnectionRef = Mode switch
            {
                TransactionMode.Query => Context.GetReadConnectionRef(),
                TransactionMode.Mutation => Context.GetWriteConnectionRef(),
                _ => throw new ArgumentOutOfRangeException()
            };

            _dbTransaction = _dbConnectionRef.Value.BeginTransaction(IsolationLevel.ReadUncommitted);
        }

        private async ValueTask StartDbTransactionAsync()
        {
            _dbConnectionRef = Mode switch
            {
                TransactionMode.Query => Context.GetReadConnectionRef(),
                TransactionMode.Mutation => Context.GetWriteConnectionRef(),
                _ => throw new ArgumentOutOfRangeException()
            };

            _dbTransaction =
                (await _dbConnectionRef.Value.BeginTransactionAsync(IsolationLevel.ReadUncommitted) as
                    Microsoft.Data.Sqlite.SqliteTransaction)!;
        }

        /// <summary>
        /// Asynchronously applies the changes made in the transaction.
        /// </summary>
        public override async Task CommitAsync()
        {
            await base.CommitAsync();
            if (_dbTransaction != null)
            {
                await _dbTransaction.CommitAsync();
            }
        }

        /// <summary>
        /// Applies the changes made in the transaction.
        /// </summary>
        public override void Commit()
        {
            base.Commit();
            _dbTransaction?.Commit();
        }

        /// <summary>
        /// Asynchronously reverts the changes made in the transaction.
        /// </summary>
        public override async Task RollbackAsync()
        {
            await base.CommitAsync();
            if (_dbTransaction != null)
            {
                await _dbTransaction.RollbackAsync();
            }
        }

        /// <summary>
        /// Reverts the changes made in the transaction.
        /// </summary>
        public override void Rollback()
        {
            base.Rollback();
            _dbTransaction?.Rollback();
        }

        /// <summary>
        /// Disposes the transaction object.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the transaction object.
        /// </summary>
        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (!_disposed)
            {
                if (_dbTransaction != null)
                {
                    await _dbTransaction.DisposeAsync();
                }

                _dbConnectionRef?.Dispose();

                _disposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _dbTransaction?.Dispose();
                _dbConnectionRef?.Dispose();
            }

            _disposed = true;
        }

        #region sql commands

        private SqliteCommand MakeDbCommand(Func<string> sqlInitializer, string name, params object[] args)
        {
            if (_dbCommandCache.Get(name, out var command))
            {
                command.Parameters.Clear();
            }
            else
            {
                command = DbConnection.CreateCommand();
                command.CommandText = sqlInitializer();
            }

            for (var i = 0; i < args.Length; i++)
            {
                command.Parameters.AddWithValue("?" + (i + 1), args[i]);
            }

            return command;
        }

        private void CacheDbCommand(string name, SqliteCommand command)
        {
            _dbCommandCache.Add(name, command);
        }

        /// <summary>
        /// Execute sqlite command. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The number of rows inserted, updated, or deleted. -1 for SELECT statements.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executenonquery"/>
        public int ExecuteNonQuery(Func<string> sqlInitializer, string name, params object[] args)
        {
            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = command.ExecuteNonQuery();
            CacheDbCommand(name, command);
            return result;
        }

        /// <summary>
        /// Execute sqlite command and returns a data reader. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="readerFunc">The reader function. The return value will be used as the return value of this method.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The data reader.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executereader"/>
        public T ExecuteReader<T>(Func<string> sqlInitializer, string name, Func<SqliteDataReader, T> readerFunc, params object[] args)
        {
            var command = MakeDbCommand(sqlInitializer, name, args);
            var reader = command.ExecuteReader();
            var result = readerFunc(reader);
            reader.Close();
            CacheDbCommand(name, command);
            return result;
        }

        /// <summary>
        /// Execute sqlite command and returns the result. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The first column of the first row of the results, or null if no results.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executescalar"/>
        public object? ExecuteScalar(Func<string> sqlInitializer, string name, params object[] args)
        {
            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = command.ExecuteScalar();
            CacheDbCommand(name, command);
            return result;
        }

        /// <summary>
        /// Execute sqlite command asynchronously. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The number of rows inserted, updated, or deleted. -1 for SELECT statements.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executenonqueryasync"/>
        public async Task<int> ExecuteNonQueryAsync(Func<string> sqlInitializer, string name, params object[] args)
        {
            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = await command.ExecuteNonQueryAsync();
            CacheDbCommand(name, command);
            return result;
        }

        /// <summary>
        /// Execute sqlite command asynchronously and returns a data reader. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="readerFunc">The reader function. The return value will be used as the return value of this method.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The data reader.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executereaderasync"/>
        public async Task<T> ExecuteReaderAsync<T>(Func<string> sqlInitializer, string name, Func<SqliteDataReader, T> readerFunc, params object[] args)
        {
            var command = MakeDbCommand(sqlInitializer, name, args);
            var reader = await command.ExecuteReaderAsync();
            var result = readerFunc(reader);
            await reader.CloseAsync();
            CacheDbCommand(name, command);
            return result;
        }

        /// <summary>
        /// Execute sqlite command asynchronously and returns the result. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The first column of the first row of the results, or null if no results.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executescalarasync"/>
        public async Task<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object[] args)
        {
            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = await command.ExecuteScalarAsync();
            CacheDbCommand(name, command);
            return result;
        }

        #endregion

        /// <summary>
        /// Finalizes an instance of the <see cref="SqliteTransaction"/> class.
        /// </summary>
        ~SqliteTransaction()
        {
            Dispose(false);
        }
    }
}
