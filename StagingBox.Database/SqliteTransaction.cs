using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using StagingBox.Utils;
using static StagingBox.Database.ITransaction;

namespace StagingBox.Database
{
    public class SqliteTransaction : BaseDbTransaction
    {
        private ObjectPool<SqliteConnection>.Ref? _dbConnectionRef;

        private Microsoft.Data.Sqlite.SqliteTransaction? _dbTransaction;

        private SqliteCommandCache _dbCommandCache = new();

        private bool _disposed;

        public SqliteConnection DbConnection
        {
            get
            {
                return _dbConnectionRef!.Value;
            }
        }

        public Microsoft.Data.Sqlite.SqliteTransaction DbTransaction
        {
            get
            {
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
            ITransaction.TransactionMode mode)
            : base(mode)
        {
            Context = context;
            StartDbTransaction();
        }

        private void StartDbTransaction()
        {
            EnsureNotCompleted();

            _dbConnectionRef = Mode switch
            {
                ITransaction.TransactionMode.Query => Context.GetReadConnectionRef(),
                ITransaction.TransactionMode.Mutation => Context.GetWriteConnectionRef(),
                ITransaction.TransactionMode.Create => Context.GetCreateConnectionRef(),
                _ => throw new ArgumentOutOfRangeException()
            };

            _dbTransaction = _dbConnectionRef.Value.BeginTransaction(
                IsolationLevel.ReadUncommitted,
                deferred: Mode == ITransaction.TransactionMode.Query);
        }

        private ValueTask StartDbTransactionAsync()
        {
            EnsureNotCompleted();

            _dbConnectionRef = Mode switch
            {
                ITransaction.TransactionMode.Query => Context.GetReadConnectionRef(),
                ITransaction.TransactionMode.Mutation => Context.GetWriteConnectionRef(),
                ITransaction.TransactionMode.Create => Context.GetCreateConnectionRef(),
                _ => throw new ArgumentOutOfRangeException()
            };

            _dbTransaction = _dbConnectionRef.Value.BeginTransaction(
                IsolationLevel.ReadUncommitted,
                deferred: Mode == ITransaction.TransactionMode.Query);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Asynchronously applies the changes made in the transaction.
        /// </summary>
        public override async Task CommitAsync()
        {
            EnsureNotCompleted();

            if (_dbTransaction != null)
            {
                await _dbTransaction.CommitAsync();
            }

            await base.CommitAsync();
        }

        /// <summary>
        /// Applies the changes made in the transaction.
        /// </summary>
        public override void Commit()
        {
            EnsureNotCompleted();

            _dbTransaction?.Commit();
            base.Commit();
        }

        /// <summary>
        /// Asynchronously reverts the changes made in the transaction.
        /// </summary>
        public override async Task RollbackAsync()
        {
            EnsureNotCompleted();

            if (_dbTransaction != null)
            {
                await _dbTransaction.RollbackAsync();
            }

            await base.RollbackAsync();
        }

        /// <summary>
        /// Reverts the changes made in the transaction.
        /// </summary>
        public override void Rollback()
        {
            EnsureNotCompleted();

            _dbTransaction?.Rollback();
            base.Rollback();
        }

        /// <summary>
        /// Disposes the transaction object.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            base.Dispose();
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
                    _dbTransaction = null;
                }

                _dbConnectionRef?.Dispose();
                _dbTransaction = null;

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
                _dbTransaction = null;
                _dbConnectionRef?.Dispose();
                _dbTransaction = null;
            }

            _disposed = true;
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

        /// <inheritdoc/>
        public override int ExecuteNonQuery(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = command.ExecuteNonQuery();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override object? ExecuteScalar(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = command.ExecuteScalar();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc/>
        public override async Task<int> ExecuteNonQueryAsync(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            var command = MakeDbCommand(sqlInitializer, name, args);
            var result = await command.ExecuteNonQueryAsync();
            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override async Task<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

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
