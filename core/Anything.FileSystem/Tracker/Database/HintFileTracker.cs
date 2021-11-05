using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Utils;
using Anything.Utils.Event;
using Anything.Utils.Logging;

namespace Anything.FileSystem.Tracker.Database
{
    /// <summary>
    ///     File tracker using sqlite database.
    ///     The index methods are serial, i.e. only one indexing task will be executed at the same time.
    /// </summary>
    public partial class HintFileTracker : Disposable
    {
        private readonly BatchEventWorkerEmitter<AttachDataEvent> _attachDataEventEmitter = new(100);
        private readonly SqliteContext _context;
        private readonly DatabaseTable _databaseTable;

        private readonly BatchEventWorkerEmitter<FileEvent> _fileChangeEventEmitter = new(100);
        private readonly ILogger _logger;

        public HintFileTracker(IStorage storage, ILogger logger)
        {
            _logger = logger.WithType<HintFileTracker>();
            _context = storage.SqliteContext;
            _databaseTable = new DatabaseTable("FileTracker");
            Create().AsTask().Wait();
        }

        public Event<FileEvent[]> FileEvent => _fileChangeEventEmitter.Event;

        public Event<AttachDataEvent[]> AttachDataEvent => _attachDataEventEmitter.Event;

        public async ValueTask CommitHint(Hint hint)
        {
            _logger.Verbose("Commiting hint {hint}", hint);
            using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
            _logger.Verbose("Indexing hint {hint}", hint);
            var eventBuilder = new FileChangeEventBuilder();
            if (hint is FileHint fileHint)
            {
                await IndexFile(transaction, fileHint.Path, fileHint.FileHandle, fileHint.FileStats, eventBuilder);
            }
            else if (hint is DeletedHint deletedHint)
            {
                await IndexDelete(transaction, deletedHint.Path, deletedHint.FileHandle, eventBuilder);
            }
            else if (hint is DirectoryHint directoryHint)
            {
                await IndexDirectory(transaction, directoryHint.Path, directoryHint.Contents, eventBuilder);
            }
            else if (hint is AttachedDataHint attachedResourceTagHint)
            {
                await IndexAttachData(
                    transaction,
                    attachedResourceTagHint.Path,
                    attachedResourceTagHint.FileHandle,
                    attachedResourceTagHint.FileStats,
                    attachedResourceTagHint.AttachedData,
                    eventBuilder);
            }

            await transaction.CommitAsync();
            await EmitFileChangeEvents(eventBuilder.BuildFileEvents());
            await EmitAttachDataEvents(eventBuilder.BuildAttachDataEvent());
        }

        public async ValueTask WaitComplete()
        {
            await _fileChangeEventEmitter.WaitComplete();
            await _attachDataEventEmitter.WaitComplete();
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
                else if (oldFile.FileStats != newFile.FileStats)
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
                    added.FileHandle,
                    added.FileStats);
                eventBuilder.Created(added.FileHandle, added.FileStats);
            }

            foreach (var updatedTagContent in updatedTagContents)
            {
                if (updatedTagContent.IsNew)
                {
                    await _databaseTable.UpdateIdentifierTagByIdAsync(
                        transaction,
                        updatedTagContent.Id,
                        updatedTagContent.FileHandle,
                        updatedTagContent.FileStats);
                    eventBuilder.Created(updatedTagContent.FileHandle, updatedTagContent.FileStats);
                }
                else
                {
                    await _databaseTable.UpdateStatsByIdAsync(
                        transaction,
                        updatedTagContent.Id,
                        updatedTagContent.FileStats);
                    eventBuilder.Changed(updatedTagContent.FileHandle, updatedTagContent.FileStats);
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
                    fileHandle,
                    fileStats);
                eventBuilder.Created(fileHandle, fileStats);
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
                    fileHandle,
                    fileStats);
                eventBuilder.Created(fileHandle, fileStats);
                return newId;
            }

            if (oldFile.IdentifierTag == null)
            {
                await _databaseTable.UpdateIdentifierTagByIdAsync(
                    transaction,
                    oldFile.Id,
                    fileHandle,
                    fileStats);
                eventBuilder.Created(fileHandle, fileStats);
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
                    fileHandle,
                    fileStats);
                eventBuilder.Created(fileHandle, fileStats);
                return newId;
            }

            if (oldFile.FileStats != fileStats)
            {
                await _databaseTable.UpdateStatsByIdAsync(
                    transaction,
                    oldFile.Id,
                    fileStats);
                return oldFile.Id;
            }

            return oldFile.Id;
        }

        private async ValueTask IndexAttachData(
            IDbTransaction transaction,
            string path,
            FileHandle fileHandle,
            FileStats fileStats,
            FileAttachedData data,
            FileChangeEventBuilder fileChangeEventBuilder)
        {
            if (path == "/")
            {
                // TODO
                throw new NotImplementedException();
            }

            var fileId = await IndexFile(transaction, path, fileHandle, fileStats, fileChangeEventBuilder);

            await _databaseTable.InsertOrReplaceAttachedDataAsync(transaction, fileId, data);
        }

        /// <summary>
        ///     Create database table.
        /// </summary>
        private async ValueTask Create()
        {
            _logger.Debug("Creating database table.");
            using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Create);
            await _databaseTable.CreateAsync(transaction);
            await transaction.CommitAsync();
            _logger.Debug("Create database table success.");
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

        private async ValueTask Delete(IDbTransaction transaction, DatabaseTable.DataRow file, FileChangeEventBuilder eventBuilder)
        {
            var attachedData = (await _databaseTable.SelectAttachedDataByTargetAsync(transaction, file.Id))
                .Select(ConvertAttachedDataDataRowToAttachedData).ToImmutableArray();

            var startsWith = file.Path;
            if (!startsWith.EndsWith("/", StringComparison.Ordinal))
            {
                startsWith += "/";
            }

            var childFiles = await _databaseTable.SelectByStartsWithAsync(transaction, startsWith);
            var childAttachedData =
                (await _databaseTable.SelectAttachedDataByStartsWithAsync(transaction, startsWith))
                .GroupBy(row => row.Target)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ConvertAttachedDataDataRowToAttachedData).ToImmutableArray());

            await _databaseTable.DeleteByPathAsync(transaction, file.Path);
            await _databaseTable.DeleteByStartsWithAsync(transaction, startsWith);

            if (file.IdentifierTag != null)
            {
                eventBuilder.Deleted(file.FileHandle!, file.FileStats!);
                eventBuilder.DeletedAttachedData(file.FileHandle!, attachedData);
            }

            foreach (var childFile in childFiles)
            {
                if (childFile.IdentifierTag != null)
                {
                    eventBuilder.Deleted(
                        file.FileHandle!,
                        file.FileStats!);
                    eventBuilder.DeletedAttachedData(
                        file.FileHandle!,
                        childAttachedData.GetValueOrDefault(childFile.Id, ImmutableArray.Create<FileAttachedData>()));
                }
            }
        }

        private async ValueTask EmitFileChangeEvents(FileEvent[] changeEvents)
        {
            if (changeEvents.Length == 0)
            {
                return;
            }

            await _fileChangeEventEmitter.EmitAsync(changeEvents);
        }

        private async ValueTask EmitAttachDataEvents(AttachDataEvent[] attachDataEvents)
        {
            if (attachDataEvents.Length == 0)
            {
                return;
            }

            await _attachDataEventEmitter.EmitAsync(attachDataEvents);
        }

        private FileAttachedData ConvertAttachedDataDataRowToAttachedData(DatabaseTable.AttachedDataDataRow dataRow)
        {
            return dataRow.FileAttachedData;
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileChangeEventEmitter.Dispose();
        }

        private class FileChangeEventBuilder
        {
            private readonly List<AttachDataEvent> _attachDataEvents = new();
            private readonly List<FileEvent> _fileEvents = new();

            public void Created(FileHandle fileHandle, FileStats fileStats)
            {
                _fileEvents.Add(
                    new FileEvent(FileSystem.FileEvent.EventType.Created, fileHandle, fileStats));
            }

            public void Changed(FileHandle fileHandle, FileStats fileStats)
            {
                _fileEvents.Add(
                    new FileEvent(FileSystem.FileEvent.EventType.Changed, fileHandle, fileStats));
            }

            public void Deleted(FileHandle fileHandle, FileStats fileStats)
            {
                _fileEvents.Add(
                    new FileEvent(FileSystem.FileEvent.EventType.Deleted, fileHandle, fileStats));
            }

            public void DeletedAttachedData(FileHandle fileHandle, ImmutableArray<FileAttachedData> attachedData)
            {
                _attachDataEvents.AddRange(attachedData.Select(
                    item => new AttachDataEvent(FileSystem.AttachDataEvent.EventType.Deleted, fileHandle, item)));
            }

            public FileEvent[] BuildFileEvents()
            {
                return _fileEvents.ToArray();
            }

            public AttachDataEvent[] BuildAttachDataEvent()
            {
                return _attachDataEvents.ToArray();
            }
        }
    }
}
