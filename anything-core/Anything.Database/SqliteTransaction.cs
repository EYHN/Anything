using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Anything.Utils;
using Microsoft.Data.Sqlite;
using static Anything.Database.ITransaction;

namespace Anything.Database
{
    public class SqliteTransaction : BaseDbTransaction
    {
        private readonly SqliteCommandCache _dbCommandCache = new();
        private readonly ObjectPool<SqliteConnection>.Ref _dbConnectionRef;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteTransaction" /> class.
        /// </summary>
        /// <param name="context">Associated context.</param>
        /// <param name="mode">Transaction mode.</param>
        public SqliteTransaction(
            SqliteContext context,
            TransactionMode mode)
            : base(mode)
        {
            Context = context;

            _dbConnectionRef = Mode switch
            {
                TransactionMode.Query => Context.GetReadConnectionRef(),
                TransactionMode.Mutation => Context.GetWriteConnectionRef(),
                TransactionMode.Create => Context.GetCreateConnectionRef(),
                _ => throw new ArgumentOutOfRangeException()
            };

            DbTransaction = _dbConnectionRef.Value.BeginTransaction(
                IsolationLevel.ReadUncommitted,
                Mode == TransactionMode.Query);
        }

        public SqliteConnection DbConnection => _dbConnectionRef.Value;

        public Microsoft.Data.Sqlite.SqliteTransaction DbTransaction { get; private set; }

        /// <summary>
        ///     Gets the associated context of this transaction.
        /// </summary>
        public SqliteContext Context { get; }

        /// <summary>
        ///     Asynchronously applies the changes made in the transaction.
        /// </summary>
        public override async Task CommitAsync()
        {
            EnsureNotCompleted();

            await DbTransaction.CommitAsync();

            await base.CommitAsync();
        }

        /// <summary>
        ///     Applies the changes made in the transaction.
        /// </summary>
        public override void Commit()
        {
            EnsureNotCompleted();

            DbTransaction.Commit();
            base.Commit();
        }

        /// <summary>
        ///     Asynchronously reverts the changes made in the transaction.
        /// </summary>
        public override async Task RollbackAsync()
        {
            EnsureNotCompleted();

            await DbTransaction.RollbackAsync();
            await base.RollbackAsync();
        }

        /// <summary>
        ///     Reverts the changes made in the transaction.
        /// </summary>
        public override void Rollback()
        {
            EnsureNotCompleted();

            DbTransaction.Rollback();
            base.Rollback();
        }

        /// <summary>
         ///     Disposes the transaction object.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Asynchronously disposes the transaction object.
        /// </summary>
        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (!_disposed)
            {
                await DbTransaction.DisposeAsync();
                _dbConnectionRef.Dispose();
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
                DbTransaction.Dispose();
                _dbConnectionRef.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="SqliteTransaction" /> class.
        /// </summary>
        ~SqliteTransaction()
        {
            Dispose(false);
        }

        #region sql commands

        private SqliteCommand MakeDbCommand(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

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
                command.Parameters.AddWithValue("?" + (i + 1), args[i] ?? DBNull.Value);
            }

            return command;
        }

        private void CacheDbCommand(string name, SqliteCommand command)
        {
            _dbCommandCache.Add(name, command);
        }

        /// <inheritdoc />
        public override int ExecuteNonQuery(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = command.ExecuteNonQuery();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override T ExecuteReader<T>(
            Func<string> sqlInitializer,
            string name,
            Func<DbDataReader, T> readerFunc,
            params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var reader = command.ExecuteReader();
            var result = readerFunc(reader);
            reader.Close();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override object? ExecuteScalar(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = command.ExecuteScalar();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override async Task<int> ExecuteNonQueryAsync(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = await command.ExecuteNonQueryAsync();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override async Task<T> ExecuteReaderAsync<T>(
            Func<string> sqlInitializer,
            string name,
            Func<DbDataReader, T> readerFunc,
            params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var reader = await command.ExecuteReaderAsync();
            var result = readerFunc(reader);
            await reader.CloseAsync();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override async Task<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = await command.ExecuteScalarAsync();
            CacheDbCommand(name, command);
            return result;
        }

        #endregion
    }
}
