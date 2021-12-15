using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem.Property;
using Anything.Utils;
using Microsoft.Extensions.Logging;

namespace Anything.FileSystem.Singleton.Tracker;

/// <summary>
///     File tracker using sqlite database.
///     The index methods are serial, i.e. only one indexing task will be executed at the same time.
/// </summary>
public partial class HintFileTracker
{
    private readonly SqliteContext _context;
    private readonly DatabaseTable _databaseTable;

    private readonly IFileEventService _fileEventService;
    private readonly ILogger _logger;

    public HintFileTracker(IStorage storage, IFileEventService fileEventService, ILogger logger)
    {
        _fileEventService = fileEventService;
        _logger = logger;
        _context = storage.SqliteContext;
        _databaseTable = new DatabaseTable("FileTracker");
        Create().AsTask().Wait();
    }

    public async ValueTask HintFile(string path, FileHandle fileHandle, FileStats fileStats)
    {
        using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
        var eventBuilder = new FileChangeEventBuilder();

        await IndexFile(transaction, path, fileHandle, fileStats, eventBuilder);

        await transaction.CommitAsync();

        await EmitFileChangeEvents(eventBuilder.BuildFileEvents());
    }

    public async ValueTask HintDirectory(string path, FileHandle fileHandle, ImmutableArray<Dirent> contents)
    {
        using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
        var eventBuilder = new FileChangeEventBuilder();

        await IndexDirectory(transaction, path, contents, eventBuilder);

        await transaction.CommitAsync();
        await EmitFileChangeEvents(eventBuilder.BuildFileEvents());
    }

    public async ValueTask HintDeleted(string path, FileHandle fileHandle)
    {
        using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
        var eventBuilder = new FileChangeEventBuilder();

        await IndexDelete(transaction, path, fileHandle, eventBuilder);

        await transaction.CommitAsync();
        await EmitFileChangeEvents(eventBuilder.BuildFileEvents());
    }

    public async ValueTask SetProperty(
        string path,
        FileHandle fileHandle,
        FileStats fileStats,
        string key,
        ReadOnlyMemory<byte> value,
        PropertyFeature feature)
    {
        using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
        var eventBuilder = new FileChangeEventBuilder();

        if (path == "/")
        {
            // TODO
            throw new NotImplementedException();
        }

        var fileId = await IndexFile(transaction, path, fileHandle, fileStats, eventBuilder);

        await _databaseTable.InsertOrReplacePropertyAsync(transaction, fileId, key, value, feature);

        await transaction.CommitAsync();
        await EmitFileChangeEvents(eventBuilder.BuildFileEvents());
    }

    public async ValueTask RemoveProperty(
        string path,
        FileHandle fileHandle,
        FileStats fileStats,
        string key)
    {
        using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
        var eventBuilder = new FileChangeEventBuilder();

        if (path == "/")
        {
            // TODO
            throw new NotImplementedException();
        }

        var fileId = await IndexFile(transaction, path, fileHandle, fileStats, eventBuilder);

        if (await _databaseTable.DeleteProperty(transaction, fileId, key))
        {
            eventBuilder.PropertyUpdated(fileHandle);
        }

        await transaction.CommitAsync();
        await EmitFileChangeEvents(eventBuilder.BuildFileEvents());
    }

    public async ValueTask<ReadOnlyMemory<byte>?> GetProperty(string path, FileHandle fileHandle, FileStats fileStats, string key)
    {
        using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
        var eventBuilder = new FileChangeEventBuilder();
        var fileId = await IndexFile(transaction, path, fileHandle, fileStats, eventBuilder);
        var property = await _databaseTable.SelectPropertyAsync(transaction, fileId, key);
        await transaction.CommitAsync();
        await EmitFileChangeEvents(eventBuilder.BuildFileEvents());
        return property?.Value;
    }

    /// <summary>
    ///     Create indexes of the contents in the directory.
    /// </summary>
    /// <param name="path">The path of the directory.</param>
    /// <param name="contents">The contents in the directory.</param>
    private async ValueTask IndexDirectory(
        IDbTransaction transaction,
        string path,
        IEnumerable<Dirent> contents,
        FileChangeEventBuilder eventBuilder)
    {
        var directoryId = await CreateDirectory(transaction, path, eventBuilder);

        var oldContents =
            (await _databaseTable.SelectByParentAsync(transaction, directoryId)).ToDictionary(content => content.Path);

        var newContents =
            contents.Select(
                    content => new
                    {
                        Path = PathLib.Join(path, content.Name),
                        Parent = directoryId,
                        IsDirectory = content.Stats.Type.HasFlag(FileType.Directory),
                        IdentifierTag = content.FileHandle.Identifier,
                        content.FileHandle,
                        FileStats = content.Stats
                    })
                .ToDictionary(content => content.Path);

        var addedContents =
            newContents.Keys.Except(oldContents.Keys).Select(key => newContents[key]).ToList();
        var removedContents =
            oldContents.Keys.Except(newContents.Keys).Select(key => oldContents[key]).ToList();
        var reservedContents =
            oldContents.Keys.Intersect(newContents.Keys).Select(
                key =>
                    new { oldContents[key].Path, oldFile = oldContents[key], newFile = newContents[key] });

        var updatedTagContents = new List<(long Id, string Path, FileHandle FileHandle, FileStats FileStats, bool IsNew)>();

        foreach (var reservedContent in reservedContents)
        {
            var oldFile = reservedContent.oldFile;
            var newFile = reservedContent.newFile;
            if (oldFile.IsDirectory != newFile.IsDirectory)
            {
                removedContents.Add(oldFile);
                addedContents.Add(newFile);
            }
            else if (oldFile.IdentifierTag == null)
            {
                updatedTagContents.Add(
                    (oldFile.Id, newFile.Path, newFile.FileHandle, newFile.FileStats, true));
            }
            else if (oldFile.IdentifierTag != newFile.IdentifierTag)
            {
                removedContents.Add(oldFile);
                addedContents.Add(newFile);
            }
            else if (oldFile.ContentTag != newFile.FileStats.Hash.ContentTag)
            {
                updatedTagContents.Add((oldFile.Id, newFile.Path, newFile.FileHandle, newFile.FileStats, false));
            }
        }

        foreach (var removed in removedContents)
        {
            await Delete(transaction, removed, eventBuilder);
        }

        foreach (var added in addedContents)
        {
            await _databaseTable.InsertAsync(
                transaction,
                added.Path,
                directoryId,
                added.IsDirectory,
                added.FileHandle.Identifier,
                added.FileStats.Hash.ContentTag);
            eventBuilder.Created(added.FileHandle);
        }

        foreach (var updatedTagContent in updatedTagContents)
        {
            if (updatedTagContent.IsNew)
            {
                await _databaseTable.UpdateIdentifierTagByIdAsync(
                    transaction,
                    updatedTagContent.Id,
                    updatedTagContent.FileHandle.Identifier,
                    updatedTagContent.FileStats.Hash.ContentTag);
                eventBuilder.Created(updatedTagContent.FileHandle);
            }
            else
            {
                await _databaseTable.UpdateStatsByIdAsync(
                    transaction,
                    updatedTagContent.Id,
                    updatedTagContent.FileStats.Hash.ContentTag);
                await _databaseTable.DeletePropertyOnFileUpdated(transaction, updatedTagContent.Id);

                eventBuilder.Changed(updatedTagContent.FileHandle);
            }
        }
    }

    private async ValueTask IndexDelete(
        IDbTransaction transaction,
        string path,
        FileHandle fileHandle,
        FileChangeEventBuilder eventBuilder)
    {
        var file = await _databaseTable.SelectByPathAsync(transaction, path);

        if (file == null)
        {
            return;
        }

        if (file.FileHandle != fileHandle)
        {
            return;
        }

        await Delete(transaction, file, eventBuilder);
    }

    private async ValueTask<long> IndexFile(
        IDbTransaction transaction,
        string path,
        FileHandle fileHandle,
        FileStats fileStats,
        FileChangeEventBuilder eventBuilder)
    {
        if (path == "/")
        {
            return 0;
        }

        var fileType = fileStats.Type;
        var isDirectory = fileStats.Type.HasFlag(FileType.Directory);

        var oldFile = await _databaseTable.SelectByPathAsync(transaction, path);

        if (oldFile == null)
        {
            long? parentId = await CreateDirectory(transaction, PathLib.Dirname(path), eventBuilder);
            var newId = await _databaseTable.InsertAsync(
                transaction,
                path,
                parentId,
                fileType.HasFlag(FileType.Directory),
                fileHandle.Identifier,
                fileStats.Hash.ContentTag);
            eventBuilder.Created(fileHandle);
            return newId;
        }

        if (oldFile.IsDirectory != isDirectory)
        {
            await Delete(transaction, oldFile, eventBuilder);
            var parentId = await CreateDirectory(transaction, PathLib.Dirname(path), eventBuilder);
            var newId = await _databaseTable.InsertAsync(
                transaction,
                path,
                parentId,
                fileType.HasFlag(FileType.Directory),
                fileHandle.Identifier,
                fileStats.Hash.ContentTag);
            eventBuilder.Created(fileHandle);
            return newId;
        }

        if (oldFile.IdentifierTag == null)
        {
            await _databaseTable.UpdateIdentifierTagByIdAsync(
                transaction,
                oldFile.Id,
                fileHandle.Identifier,
                fileStats.Hash.ContentTag);
            eventBuilder.Created(fileHandle);
            return oldFile.Id;
        }

        if (oldFile.IdentifierTag != fileHandle.Identifier)
        {
            await Delete(transaction, oldFile, eventBuilder);
            var parentId = await CreateDirectory(transaction, PathLib.Dirname(path), eventBuilder);
            var newId = await _databaseTable.InsertAsync(
                transaction,
                path,
                parentId,
                fileType.HasFlag(FileType.Directory),
                fileHandle.Identifier,
                fileStats.Hash.ContentTag);
            eventBuilder.Created(fileHandle);
            return newId;
        }

        if (oldFile.ContentTag != fileStats.Hash.ContentTag)
        {
            await _databaseTable.UpdateStatsByIdAsync(
                transaction,
                oldFile.Id,
                fileStats.Hash.ContentTag);
            await _databaseTable.DeletePropertyOnFileUpdated(transaction, oldFile.Id);

            eventBuilder.Changed(fileHandle);
            return oldFile.Id;
        }

        return oldFile.Id;
    }

    /// <summary>
    ///     Create database table.
    /// </summary>
    private async ValueTask Create()
    {
        _logger.LogDebug("Creating database table.");
        using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Create);
        await _databaseTable.CreateAsync(transaction);
        await transaction.CommitAsync();
        _logger.LogDebug("Create database table success.");
    }

    private async ValueTask<long> CreateDirectory(IDbTransaction transaction, string path, FileChangeEventBuilder eventBuilder)
    {
        var pathPart = PathLib.Split(path);
        long? directoryId = null;
        int i;

        for (i = pathPart.Length; i >= 0; i--)
        {
            var currentPath = "/" + string.Join('/', pathPart.Take(i).ToArray());
            var directory = await _databaseTable.SelectByPathAsync(transaction, currentPath);
            if (directory == null)
            {
                continue;
            }

            if (directory.IsDirectory)
            {
                directoryId = directory.Id;
                break;
            }

            await Delete(transaction, directory, eventBuilder);
        }

        for (i++; i <= pathPart.Length; i++)
        {
            var currentPath = "/" + string.Join('/', pathPart.Take(i).ToArray());
            directoryId = await _databaseTable.InsertAsync(transaction, currentPath, directoryId, true, null, null);
        }

        return directoryId!.Value;
    }

    private async ValueTask Delete(
        IDbTransaction transaction,
        DatabaseTable.DataRow file,
        FileChangeEventBuilder eventBuilder)
    {
        var startsWith = file.Path;
        if (!startsWith.EndsWith("/", StringComparison.Ordinal))
        {
            startsWith += "/";
        }

        var childFiles = await _databaseTable.SelectByStartsWithAsync(transaction, startsWith);

        await _databaseTable.DeleteByPathAsync(transaction, file.Path);
        await _databaseTable.DeleteByStartsWithAsync(transaction, startsWith);

        if (file.IdentifierTag != null)
        {
            eventBuilder.Deleted(file.FileHandle!);
        }

        foreach (var childFile in childFiles)
        {
            if (childFile.IdentifierTag != null)
            {
                eventBuilder.Deleted(
                    file.FileHandle!);
            }
        }
    }

    private async ValueTask EmitFileChangeEvents(FileEvent[] changeEvents)
    {
        if (changeEvents.Length == 0)
        {
            return;
        }

        await _fileEventService.Emit(changeEvents);
    }

    private class FileChangeEventBuilder
    {
        private readonly List<FileEvent> _fileEvents = new();

        public void Created(FileHandle fileHandle)
        {
            _fileEvents.Add(
                new FileEvent(FileEvent.EventType.Created, fileHandle));
        }

        public void Changed(FileHandle fileHandle)
        {
            _fileEvents.Add(
                new FileEvent(FileEvent.EventType.Changed, fileHandle));
        }

        public void Deleted(FileHandle fileHandle)
        {
            _fileEvents.Add(
                new FileEvent(FileEvent.EventType.Deleted, fileHandle));
        }

        public void PropertyUpdated(FileHandle fileHandle)
        {
            _fileEvents.Add(
                new FileEvent(FileEvent.EventType.PropertyUpdated, fileHandle));
        }

        public FileEvent[] BuildFileEvents()
        {
            return _fileEvents.ToArray();
        }
    }
}
