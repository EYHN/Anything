using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Utils;

namespace Anything.FileSystem.Tracker.Database
{
    /// <summary>
    ///     File tracker using sqlite database.
    ///     The index methods are serial, i.e. only one indexing task will be executed at the same time.
    /// </summary>
    public class DatabaseFileTracker : IFileTracker
    {
        private readonly SqliteContext _context;
        private readonly FileTable _fileTable;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseFileTracker" /> class.
        /// </summary>
        /// <param name="context">The sqlite context.</param>
        public DatabaseFileTracker(SqliteContext context)
        {
            _context = context;
            _fileTable = new FileTable("FileTracker");
        }

        /// <inheritdoc />
        public async ValueTask IndexDirectory(Url url, (string Name, FileRecord Record)[] contents)
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
            var eventBuilder = new FileChangeEventBuilder();
            var directoryId = await CreateDirectory(transaction, url, eventBuilder);

            var oldContents =
                (await _fileTable.SelectByParentAsync(transaction, directoryId)).ToDictionary(content => content.Url);

            var newContents =
                contents.Select(
                        content => new
                        {
                            Url = (url with { Path = PathLib.Join(url.Path, content.Name) }).ToString(),
                            Parent = directoryId,
                            IsDirectory = content.Record.Type.HasFlag(FileType.Directory),
                            content.Record.IdentifierTag,
                            content.Record.ContentTag
                        })
                    .ToDictionary(content => content.Url);

            var addedContents =
                newContents.Keys.Except(oldContents.Keys).Select(key => newContents[key]).ToList();
            var removedContents =
                oldContents.Keys.Except(newContents.Keys).Select(key => oldContents[key]).ToList();
            var reservedContents =
                oldContents.Keys.Intersect(newContents.Keys).Select(
                    key =>
                        new { oldContents[key].Url, oldFile = oldContents[key], newFile = newContents[key] });

            var updatedTagContents = new List<(long Id, string Url, string? IdentifierTag, string ContentTag)>();

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
                        (oldFile.Id, newFile.Url, newFile.IdentifierTag, newFile.ContentTag));
                }
                else if (oldFile.IdentifierTag != newFile.IdentifierTag)
                {
                    removedContents.Add(oldFile);
                    addedContents.Add(newFile);
                }
                else if (oldFile.ContentTag != newFile.ContentTag)
                {
                    updatedTagContents.Add((oldFile.Id, newFile.Url, null, newFile.ContentTag));
                }
            }

            foreach (var removed in removedContents)
            {
                await DeleteByStartsWith(transaction, removed.Url, eventBuilder);
            }

            foreach (var added in addedContents)
            {
                await _fileTable.InsertAsync(
                    transaction,
                    added.Url,
                    directoryId,
                    added.IsDirectory,
                    added.IdentifierTag,
                    added.ContentTag);
                eventBuilder.Created(Url.Parse(added.Url));
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
                    eventBuilder.Created(Url.Parse(updatedTagContent.Url));
                }
                else
                {
                    var trackers =
                        (await _fileTable.SelectTrackTagsByTargetAsync(transaction, updatedTagContent.Id))
                        .Select(ConvertTrackTagDataRowToTrackTag)
                        .ToArray();
                    await _fileTable.UpdateContentTagByIdAsync(
                        transaction,
                        updatedTagContent.Id,
                        updatedTagContent.ContentTag);
                    eventBuilder.Changed(Url.Parse(updatedTagContent.Url), trackers);
                }
            }

            await transaction.CommitAsync();
            EmitFileChangeEvent(eventBuilder.Build());
        }

        /// <inheritdoc />
        public async ValueTask IndexFile(Url url, FileRecord? record)
        {
            if (url.Path == "/")
            {
                return;
            }

            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
            var eventBuilder = new FileChangeEventBuilder();
            if (record == null)
            {
                await DeleteByStartsWith(transaction, url.ToString(), eventBuilder);
            }
            else
            {
                var isDirectory = record.Type.HasFlag(FileType.Directory);

                var oldFile = await _fileTable.SelectByUrlAsync(transaction, url.ToString());

                if (oldFile == null)
                {
                    long? parentId = await CreateDirectory(transaction, url.Dirname(), eventBuilder);
                    await _fileTable.InsertAsync(
                        transaction,
                        url.ToString(),
                        parentId,
                        isDirectory,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(url);
                }
                else if (oldFile.IsDirectory != isDirectory)
                {
                    await DeleteByStartsWith(transaction, url.ToString(), eventBuilder);
                    var parentId = await CreateDirectory(transaction, url.Dirname(), eventBuilder);
                    await _fileTable.InsertAsync(
                        transaction,
                        url.ToString(),
                        parentId,
                        isDirectory,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(url);
                }
                else if (oldFile.IdentifierTag == null)
                {
                    await _fileTable.UpdateIdentifierAndContentTagByIdAsync(
                        transaction,
                        oldFile.Id,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(url);
                }
                else if (oldFile.IdentifierTag != record.IdentifierTag)
                {
                    await DeleteByStartsWith(transaction, url.ToString(), eventBuilder);
                    var parentId = await CreateDirectory(transaction, url.Dirname(), eventBuilder);
                    await _fileTable.InsertAsync(
                        transaction,
                        url.ToString(),
                        parentId,
                        isDirectory,
                        record.IdentifierTag,
                        record.ContentTag);
                    eventBuilder.Created(url);
                }
                else if (oldFile.ContentTag != record.ContentTag)
                {
                    var trackers =
                        (await _fileTable.SelectTrackTagsByTargetAsync(transaction, oldFile.Id)).Select(ConvertTrackTagDataRowToTrackTag)
                        .ToArray();
                    await _fileTable.UpdateContentTagByIdAsync(
                        transaction,
                        oldFile.Id,
                        record.ContentTag);
                    eventBuilder.Changed(url, trackers);
                }
            }

            await transaction.CommitAsync();
            EmitFileChangeEvent(eventBuilder.Build());
        }

        /// <inheritdoc />
        public async ValueTask AttachTag(Url url, FileTrackTag trackTag, bool replace = false)
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
            var file = await _fileTable.SelectByUrlAsync(transaction, url.ToString());

            if (file == null)
            {
                throw new ArgumentException("The url must have been indexed", nameof(url));
            }

            if (!replace)
            {
                await _fileTable.InsertTrackTagAsync(transaction, file.Id, trackTag.Key, trackTag.Data);
            }
            else
            {
                await _fileTable.InsertOrReplaceTrackTagAsync(transaction, file.Id, trackTag.Key, trackTag.Data);
            }

            await transaction.CommitAsync();
        }

        public async ValueTask<FileTrackTag[]> GetTags(Url url)
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query);
            var file = await _fileTable.SelectByUrlAsync(transaction, url.ToString());

            if (file == null)
            {
                throw new ArgumentException("The url must have been indexed", nameof(url));
            }

            var dataRows = await _fileTable.SelectTrackTagsByTargetAsync(transaction, file.Id);
            return dataRows.Select(ConvertTrackTagDataRowToTrackTag).ToArray();
        }

        /// <inheritdoc />
        public event IFileTracker.ChangeEventHandler? OnFileChange;

        /// <summary>
        ///     Create database table.
        /// </summary>
        public async ValueTask Create()
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Create);
            await _fileTable.CreateAsync(transaction);
            await transaction.CommitAsync();
        }

        private async ValueTask<long> CreateDirectory(IDbTransaction transaction, Url url, FileChangeEventBuilder eventBuilder)
        {
            var pathPart = PathLib.Split(url.Path);
            long? directoryId = null;
            int i;

            for (i = pathPart.Length; i >= 0; i--)
            {
                var currentUrl = url with { Path = "/" + string.Join('/', pathPart.Take(i).ToArray()) };
                var directory = await _fileTable.SelectByUrlAsync(transaction, currentUrl.ToString());
                if (directory == null)
                {
                    continue;
                }

                if (directory.IsDirectory)
                {
                    directoryId = directory.Id;
                    break;
                }

                await DeleteByStartsWith(transaction, currentUrl.ToString(), eventBuilder);
            }

            for (i++; i <= pathPart.Length; i++)
            {
                var currentUrl = url with { Path = "/" + string.Join('/', pathPart.Take(i).ToArray()) };
                directoryId = await _fileTable.InsertAsync(transaction, currentUrl.ToString(), directoryId, true, null, null);
            }

            return directoryId!.Value;
        }

        private async ValueTask DeleteByStartsWith(IDbTransaction transaction, string startsWith, FileChangeEventBuilder eventBuilder)
        {
            var deleteFiles = await _fileTable.SelectByStartsWithAsync(transaction, startsWith);
            var deleteTrackTags =
                (await _fileTable.SelectTrackTagsByStartsWithAsync(transaction, startsWith))
                .GroupBy(row => row.Target)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ConvertTrackTagDataRowToTrackTag).ToArray());
            await _fileTable.DeleteByStartsWithAsync(transaction, startsWith);
            foreach (var deleteFile in deleteFiles)
            {
                if (deleteFile.IdentifierTag != null)
                {
                    eventBuilder.Deleted(
                        Url.Parse(deleteFile.Url),
                        deleteTrackTags.GetValueOrDefault(deleteFile.Id, Array.Empty<FileTrackTag>()));
                }
            }
        }

        private void EmitFileChangeEvent(FileChangeEvent[] changeEvents)
        {
            if (OnFileChange != null)
            {
                OnFileChange(changeEvents);
            }
        }

        private FileTrackTag ConvertTrackTagDataRowToTrackTag(FileTable.TrackTagDataRow row)
        {
            return new(row.Key, row.Data);
        }

        private class FileChangeEventBuilder
        {
            private readonly List<FileChangeEvent> _events = new();

            public void Created(Url url)
            {
                _events.Add(
                    new FileChangeEvent(FileChangeEvent.EventType.Created, url));
            }

            public void Changed(Url url, FileTrackTag[] trackTags)
            {
                _events.Add(
                    new FileChangeEvent(FileChangeEvent.EventType.Changed, url, trackTags));
            }

            public void Deleted(Url url, FileTrackTag[] trackTags)
            {
                _events.Add(
                    new FileChangeEvent(FileChangeEvent.EventType.Deleted, url, trackTags));
            }

            public FileChangeEvent[] Build()
            {
                return _events.ToArray();
            }
        }
    }
}
