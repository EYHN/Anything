using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Anything.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Anything.Database
{
    public class SqliteTransaction : BaseDbTransaction
    {
        private readonly SqliteCommandCache _dbCommandCache = new();
        private readonly ObjectPool<SqliteConnection>.Ref _dbConnectionRef;
        private bool _busy;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteTransaction" /> class.
        /// </summary>
        /// <param name="context">Associated context.</param>
        /// <param name="mode">Transaction mode.</param>
        /// <param name="isolated">Whether the transaction uses an isolated connection.</param>
        /// <param name="logger">Logger for the transaction.</param>
        public SqliteTransaction(
            SqliteContext context,
            ITransaction.TransactionMode mode,
            bool isolated = false,
            ILogger? logger = null)
            : base(mode, logger)
        {
            Context = context;

            _dbConnectionRef = mode switch
            {
                ITransaction.TransactionMode.Query => Context.GetReadConnectionRef(isolated),
                ITransaction.TransactionMode.Mutation => Context.GetWriteConnectionRef(isolated),
                ITransaction.TransactionMode.Create => Context.GetCreateConnectionRef(isolated),
                _ => throw new ArgumentOutOfRangeException(nameof(mode))
            };

            DbTransaction = _dbConnectionRef.Value.BeginTransaction(
                IsolationLevel.ReadUncommitted,
                Mode == ITransaction.TransactionMode.Query);
        }

        public SqliteConnection DbConnection => _dbConnectionRef.Value;

        public Microsoft.Data.Sqlite.SqliteTransaction DbTransaction { get; }

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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    DbTransaction.Dispose();
                    _dbConnectionRef.Dispose();
                }

                _disposed = false;
            }
        }

        public static string EscapeLikeContent(string content)
        {
            return content.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
        }

        private void EnterBusy()
        {
            if (_busy)
            {
                throw new InvalidOperationException("Sqlite transaction is busy.");
            }

            _busy = true;
        }

        private void LeaveBusy()
        {
            _busy = false;
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
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = sqlInitializer();
#pragma warning restore CA2100
            }

            var parameterIndex = 1;

            foreach (var arg in args)
            {
                if (arg is IEnumerable<KeyValuePair<string, object?>> parametersDictionary)
                {
                    foreach (var parameter in parametersDictionary)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                    }
                }
                else
                {
                    command.Parameters.AddWithValue("?" + parameterIndex, arg ?? DBNull.Value);
                    parameterIndex++;
                }
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

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);
            int result;

            EnterBusy();
            try
            {
                result = command.ExecuteNonQuery();
            }
            finally
            {
                LeaveBusy();
            }

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

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);
            T? result;

            EnterBusy();
            try
            {
                using var reader = command.ExecuteReader();
                result = readerFunc(reader);
            }
            finally
            {
                LeaveBusy();
            }

            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override IEnumerable<T> ExecuteEnumerable<T>(
            Func<string> sqlInitializer,
            string name,
            Func<DbDataReader, T> readerFunc,
            params object?[] args)
        {
            EnsureNotCompleted();

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);

            EnterBusy();
            try
            {
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    yield return readerFunc(reader);
                }
            }
            finally
            {
                LeaveBusy();
            }

            CacheDbCommand(name, command);
        }

        /// <inheritdoc />
        public override object? ExecuteScalar(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);
            object? result;

            EnterBusy();
            try
            {
                result = command.ExecuteScalar();
            }
            finally
            {
                LeaveBusy();
            }

            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override async Task<int> ExecuteNonQueryAsync(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);
            int result;

            EnterBusy();
            try
            {
                result = await command.ExecuteNonQueryAsync();
            }
            finally
            {
                LeaveBusy();
            }

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

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);
            T? result;

            EnterBusy();
            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                result = readerFunc(reader);
            }
            finally
            {
                LeaveBusy();
            }

            CacheDbCommand(name, command);
            return result;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<T> ExecuteEnumerableAsync<T>(
            Func<string> sqlInitializer,
            string name,
            Func<DbDataReader, T> readerFunc,
            params object?[] args)
        {
            EnsureNotCompleted();

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);
            EnterBusy();
            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    yield return readerFunc(reader);
                }
            }
            finally
            {
                LeaveBusy();
            }

            CacheDbCommand(name, command);
        }

        /// <inheritdoc />
        public override async Task<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object?[] args)
        {
            EnsureNotCompleted();

            Logger?.LogTrace($"Execute: {name}");
            var command = MakeDbCommand(sqlInitializer, name, args);
            object? result;
            EnterBusy();
            try
            {
                result = await command.ExecuteScalarAsync();
            }
            finally
            {
                LeaveBusy();
            }

            CacheDbCommand(name, command);
            return result;
        }

        #endregion
    }
}
