using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace StagingBox.Database
{
    public interface IDbTransaction : ITransaction
    {
        /// <summary>
        /// Execute db command. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The number of rows inserted, updated, or deleted. -1 for SELECT statements.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executenonquery"/>
        int ExecuteNonQuery(Func<string> sqlInitializer, string name, params object?[] args);

        /// <summary>
        /// Execute db command and returns a data reader. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="readerFunc">The reader function. The return value will be used as the return value of this method.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The data reader.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executereader"/>
        T ExecuteReader<T>(Func<string> sqlInitializer, string name, Func<DbDataReader, T> readerFunc, params object?[] args);

        /// <summary>
        /// Execute db command and returns the result. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The first column of the first row of the results, or null if no results.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executescalar"/>
        object? ExecuteScalar(Func<string> sqlInitializer, string name, params object?[] args);

        /// <summary>
        /// Execute db command asynchronously. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The number of rows inserted, updated, or deleted. -1 for SELECT statements.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executenonqueryasync"/>
        Task<int> ExecuteNonQueryAsync(Func<string> sqlInitializer, string name, params object?[] args);

        /// <summary>
        /// Execute db command asynchronously and returns a data reader. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="readerFunc">The reader function. The return value will be used as the return value of this method.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The data reader.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executereaderasync"/>
        Task<T> ExecuteReaderAsync<T>(
            Func<string> sqlInitializer,
            string name,
            Func<DbDataReader, T> readerFunc,
            params object?[] args);

        /// <summary>
        /// Execute db command asynchronously and returns the result. Can use the name to cache the commands. Use SQLite's "prepare statement" feature to improve performance.
        /// </summary>
        /// <param name="sqlInitializer">The function that generates the sql string.</param>
        /// <param name="name">The name of the command, used to cache the command.</param>
        /// <param name="args">Sql command execution parameters.</param>
        /// <returns>The first column of the first row of the results, or null if no results.</returns>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executescalarasync"/>
        Task<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object?[] args);
    }
}
