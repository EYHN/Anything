using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.Database;
using OwnHub.Utils;

namespace OwnHub.FileSystem.Indexer.Database
{
    /// <summary>
    /// File indexer using sqlite database.
    /// The index methods are serial, i.e. only one indexing task will be executed at the same time.
    /// </summary>
    public class DatabaseFileIndexer : IFileIndexer
    {
        private readonly FileTable _fileTable;

        private readonly SqliteContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFileIndexer"/> class.
        /// </summary>
        /// <param name="context">The sqlite context.</param>
        /// <param name="tableName">The table name.</param>
        public DatabaseFileIndexer(SqliteContext context, string tableName)
        {
            _context = context;
            _fileTable = new FileTable(tableName);
        }

        /// <summary>
        /// Create database table.
        /// </summary>
        public async ValueTask Create()
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Create);
            await _fileTable.CreateAsync(transaction);
            await transaction.CommitAsync();
        }

        /// <inheritdoc/>
        public async ValueTask IndexDirectory(string path, (string Name, FileRecord Record)[] contents)
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
            var eventBuilder = new FileChangeEventBuilder();
            var directoryId = await CreateDirectory(transaction, path, eventBuilder);

            var oldContents =
                (await _fileTable.SelectByParentAsync(transaction, directoryId)).ToDictionary((content) => content.Path);

            var newContents =
                contents.Select(
                        (content) => new
                        {
                            Path = PathLib.Join(path, content.Name),
                            Parent = directoryId,
                            IsDirectory = content.Record.Type.HasFlag(FileType.Directory),
                            IdentifierTag = content.Record.IdentifierTag,
                            ContentTag = content.Record.ContentTag
                        })
                    .ToDictionary((content) => content.Path);

            var addedContents =
                newContents.Keys.Except(oldContents.Keys).Select((key) => newContents[key]).ToList();
            var removedContents =
                oldContents.Keys.Except(newContents.Keys).Select((key) => oldContents[key]).ToList();
            var reservedContents =
                oldContents.Keys.Intersect(newContents.Keys).Select(
                    (key) =>
                        new { Path = oldContents[key].Path, oldFile = oldContents[key], newFile = newContents[key] });

            var updatedTagContents = new List<(long Id, string Path, string? IdentifierTag, string ContentTag)>();

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
                        (oldFile.Id, newFile.Path, newFile.IdentifierTag, newFile.ContentTag));
                }
                else if (oldFile.IdentifierTag != newFile.IdentifierTag)
                {
                    removedContents.Add(oldFile);
                    addedContents.Add(newFile);
                }
                else if (oldFile.ContentTag != newFile.ContentTag)
                {
                    updatedTagContents.Add((oldFile.Id, newFile.Path, null, newFile.ContentTag));
                }
            }

            foreach (var removed in removedContents)
            {
                await DeleteByStartsWith(transaction, removed.Path, eventBuilder);
            }

            foreach (var added in addedContents)
            {
                await _fileTable.InsertAsync(
                    transaction,
                    added.Path,
                    directoryId,
                    added.IsDirectory,
                    added.IdentifierTag,
                    added.ContentTag);
                eventBuilder.Created(added.Path);
            }

            foreach (var updatedTagContent in updatedTagContents)
            {
                if (updatedTagContent.IdentifierTag != null)
                {
                    await _fileTable.UpdateIdentifierAndContentTagByIdAsync(
                        transaction,
                        updatedTagContent.Id,
                        updatedTagContent.IdentifierTag,
                        updatedTagContent.ContentTag);
                    eventBuilder.Created(updatedTagContent.Path);
                }
                else
                {
                    await _fileTable.UpdateContentTagByIdAsync(
                        transaction,
                        updatedTagContent.Id,
                        updatedTagContent.ContentTag);
                    eventBuilder.Changed(updatedTagContent.Path);
                }
            }

            await transaction.CommitAsync();
            EmitFileChangeEvent(eventBuilder.Build());
        }

        /// <inheritdoc/>
        public async ValueTask IndexFile(string path, FileRecord? record)
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
            var eventBuilder = new FileChangeEventBuilder();
            if (record == null)
            {
                await DeleteByStartsWith(transaction, path, eventBuilder);
            }
            else
            {
                var isDirectory = record.Type.HasFlag(FileType.Directory);

                var oldFile = await _fileTable.SelectByPathAsync(transaction, path);

                if (oldFile == null)
                {
                    var parentId = await CreateDirectory(transaction, PathLib.Dirname(path), eventBuilder);
                    await _fileTable.InsertAsync(
                        transaction,
                        path,
                        parentId,
                        isDirectory,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(path);
                }
                else if (oldFile.IsDirectory != isDirectory)
                {
                    await DeleteByStartsWith(transaction, path, eventBuilder);
                    var parentId = await CreateDirectory(transaction, PathLib.Dirname(path), eventBuilder);
                    await _fileTable.InsertAsync(
                        transaction,
                        path,
                        parentId,
                        isDirectory,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(path);
                }
                else if (oldFile.IdentifierTag == null)
                {
                    await _fileTable.UpdateIdentifierAndContentTagByIdAsync(
                        transaction,
                        oldFile.Id,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(path);
                }
                else if (oldFile.IdentifierTag != record.IdentifierTag)
                {
                    await DeleteByStartsWith(transaction, path, eventBuilder);
                    var parentId = await CreateDirectory(transaction, PathLib.Dirname(path), eventBuilder);
                    await _fileTable.InsertAsync(
                        transaction,
                        path,
                        parentId,
                        isDirectory,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(path);
                }
                else if (oldFile.ContentTag != record.ContentTag)
                {
                    await _fileTable.UpdateContentTagByIdAsync(
                        transaction,
                        oldFile.Id,
                        record.ContentTag);
                    eventBuilder.Changed(path);
                }
            }

            await transaction.CommitAsync();
            EmitFileChangeEvent(eventBuilder.Build());
        }

        public event IFileIndexer.FileChangeEventHandler? OnFileChange;

        private async ValueTask<long> CreateDirectory(IDbTransaction transaction, string path, FileChangeEventBuilder eventBuilder)
        {
            var pathPart = PathLib.Split(path);
            long? directoryId = null;
            int i;

            for (i = pathPart.Length; i >= 0; i--)
            {
                var currentPath = "/" + string.Join("/", pathPart.Take(i).ToArray());
                var directory = await _fileTable.SelectByPathAsync(transaction, currentPath);
                if (directory == null)
                {
                    continue;
                }

                if (directory.IsDirectory)
                {
                    directoryId = directory.Id;
                    break;
                }
                else
                {
                    await DeleteByStartsWith(transaction, currentPath, eventBuilder);
                }
            }

            for (i++; i <= pathPart.Length; i++)
            {
                var currentPath = '/' + string.Join('/', pathPart.Take(i).ToArray());
                directoryId = await _fileTable.InsertAsync(transaction, currentPath, directoryId, true, null, null);
            }

            return directoryId!.Value;
        }

        private async ValueTask DeleteByStartsWith(IDbTransaction transaction, string startsWithPath, FileChangeEventBuilder eventBuilder)
        {
            var deleteFiles = await _fileTable.SelectByStartsWithAsync(transaction, startsWithPath);
            await _fileTable.DeleteByStartsWithAsync(transaction, startsWithPath);
            foreach (var deleteFile in deleteFiles)
            {
                if (deleteFile.IdentifierTag != null)
                {
                    eventBuilder.Deleted(deleteFile.Path);
                }
            }
        }

        private class FileChangeEventBuilder
        {
            private readonly List<string> _created = new();

            private readonly List<string> _changed = new();

            private readonly List<string> _deleted = new();

            public void Created(string path)
            {
                _created.Add(path);
            }

            public void Changed(string path)
            {
                _changed.Add(path);
            }

            public void Deleted(string path)
            {
                _deleted.Add(path);
            }

            public IFileIndexer.FileChangeEvent Build()
            {
                return new(_created.ToArray(), _deleted.ToArray(), _changed.ToArray());
            }
        }

        private void EmitFileChangeEvent(IFileIndexer.FileChangeEvent fileChangeEvent)
        {
            if (OnFileChange != null)
            {
                OnFileChange(fileChangeEvent);
            }
        }
    }
}
