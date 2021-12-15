using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Nito.Disposables;

namespace Anything.Database;

/// <summary>
///     Cache for <see cref="SqliteCommand" />.
/// </summary>
public class SqliteCommandCache : SingleDisposable<object?>
{
    private readonly Dictionary<string, SqliteCommand> _cache = new();

    public SqliteCommandCache()
        : base(null)
    {
    }

    /// <summary>
    ///     Add command to the cache. This method doesn't throw an exception if the command with the given key exists in the cache.
    /// </summary>
    /// <param name="key">The key of the command to add.</param>
    /// <param name="command">The command to add.</param>
    public void Add(string key, SqliteCommand command)
    {
        ThrowsIfDisposed();
        _cache.TryAdd(key, command);
    }

    /// <summary>
    ///     Get the command associated with the specified key. In order to prevent multiple threads from using the same command at the same time,
    ///     the command will be removed from the cache.
    /// </summary>
    /// <param name="key">The key of the command to remove.</param>
    /// <param name="command">The removed command.</param>
    /// <returns>true if the command is successfully found and removed; otherwise, false.</returns>
    public bool Get(string key, [MaybeNullWhen(false)] out SqliteCommand command)
    {
        ThrowsIfDisposed();
        return _cache.Remove(key, out command);
    }

    protected override void Dispose(object? context)
    {
        foreach (var command in _cache.Values)
        {
            command.Dispose();
        }
    }

    private void ThrowsIfDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
}
