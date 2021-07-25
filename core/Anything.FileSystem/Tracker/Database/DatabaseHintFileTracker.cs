using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Utils;
using Anything.Utils.Event;

namespace Anything.FileSystem.Tracker.Database
{
    /// <summary>
    ///     File tracker using sqlite database.
    ///     The index methods are serial, i.e. only one indexing task will be executed at the same time.
    /// </summary>
    public partial class DatabaseHintFileTracker : IHintFileTracker, IDisposable
    {
        private readonly SqliteContext _context;

        private readonly EventEmitter<FileEvent[]> _fileChangeEventEmitter = new();
        private readonly Channel<FileEvent[]> _fileChangeEventQueue = Channel.CreateBounded<FileEvent[]>(100);
        private readonly FileTable _fileTable;
        private readonly Channel<Hint> _hintQueue = Channel.CreateBounded<Hint>(100);
        private bool _disposed;
        private bool _fileChangeEventConsumerBusy;
        private Task<Task>? _fileChangeEventConsumerTask;
        private bool _hintConsumerBusy;
        private Task<Task>? _hintConsumerTask;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseHintFileTracker" /> class.
        /// </summary>
        /// <param name="context">The sqlite context.</param>
        public DatabaseHintFileTracker(SqliteContext context)
        {
            _context = context;
            _fileTable = new FileTable("FileTracker");
            Create().AsTask().Wait();
            SetupHintConsumer();
            SetupFileChangeConsumer();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask CommitHint(Hint hint)
        {
            await _hintQueue.Writer.WriteAsync(hint);
        }

        /// <inheritdoc />
        public async ValueTask WaitComplete()
        {
            while (_hintQueue.Reader.Count > 0 ||
                   _fileChangeEventQueue.Reader.Count > 0 ||
                   _hintConsumerBusy ||
                   _fileChangeEventConsumerBusy)
            {
                await Task.Delay(1);
            }
        }

        /// <inheritdoc />
        public ValueTask AttachData(Url url, FileRecord fileRecord, FileAttachedData data)
        {
            return _hintQueue.Writer.WriteAsync(new AttachedResourceTagHint(url, fileRecord, data));
        }

        /// <inheritdoc />
        public Event<FileEvent[]> FileEvent => _fileChangeEventEmitter.Event;

        private void SetupHintConsumer()
        {
            _hintConsumerTask = Task.Factory.StartNew(
                async () =>
                {
                    while (await _hintQueue.Reader.WaitToReadAsync())
                    {
                        _hintConsumerBusy = true;

                        if (_hintQueue.Reader.TryRead(out var hint))
                        {
                            try
                            {
                                await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
                                var eventBuilder = new FileChangeEventBuilder();
                                if (hint is FileHint fileHint)
                                {
                                    await IndexFile(fileHint.Url, fileHint.FileRecord, transaction, eventBuilder);
                                }
                                else if (hint is DeletedHint deletedHint)
                                {
                                    await IndexFile(deletedHint.Url, null, transaction, eventBuilder);
                                }
                                else if (hint is DirectoryHint directoryHint)
                                {
                                    await IndexDirectory(directoryHint.Url, directoryHint.Contents, transaction, eventBuilder);
                                }
                                else if (hint is AttachedResourceTagHint attachedResourceTagHint)
                                {
                                    await IndexAttachData(
                                        attachedResourceTagHint.Url,
                                        attachedResourceTagHint.FileRecord,
                                        attachedResourceTagHint.AttachedData,
                                        transaction,
                                        eventBuilder);
                                }

                                await transaction.CommitAsync();
                                await EmitFileChangeEvent(eventBuilder.Build());
                            }
                            catch (System.Exception ex)
                            {
                                Console.Error.WriteLine(ex);
                            }
                        }

                        _hintConsumerBusy = false;
                    }
                },
                default,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private void SetupFileChangeConsumer()
        {
            _fileChangeEventConsumerTask = Task.Factory.StartNew(
                async () =>
                {
                    while (await _fileChangeEventQueue.Reader.WaitToReadAsync())
                    {
                        _fileChangeEventConsumerBusy = true;
                        try
                        {
                            List<FileEvent> events = new();
                            while (_fileChangeEventQueue.Reader.TryRead(out var nextEvents))
                            {
                                events.AddRange(nextEvents);
                            }

                            if (events.Count == 0)
                            {
                                continue;
                            }

                            await _fileChangeEventEmitter.EmitAsync(events.ToArray());
                        }
                        catch (System.Exception ex)
                        {
                            Console.Error.WriteLine(ex);
                        }

                        _fileChangeEventConsumerBusy = false;
                    }
                },
                new CancellationToken(false),
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        /// <summary>
        ///     Create indexes of the contents in the directory.
        /// </summary>
        /// <param name="url">The url of the directory.</param>
        /// <param name="contents">The contents in the directory.</param>
        private async ValueTask IndexDirectory(
            Url url,
            IEnumerable<(string Name, FileRecord Record)> contents,
            IDbTransaction transaction,
            FileChangeEventBuilder eventBuilder)
        {
            var directoryId = await CreateDirectory(transaction, url, eventBuilder);

            var oldContents =
                await _fileTable.SelectByParentAsync(transaction, directoryId).ToDictionaryAsync(content => content.Url);

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
                await Delete(transaction, Url.Parse(removed.Url), eventBuilder);
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
                    var attachedDataDataRows = await _fileTable.SelectAttachedDataByTargetAsync(transaction, updatedTagContent.Id);
                    var attachedData = attachedDataDataRows
                        .Select(ConvertAttachedDataDataRowToAttachedData)
                        .ToImmutableArray();
                    await _fileTable.UpdateContentTagByIdAsync(
                        transaction,
                        updatedTagContent.Id,
                        updatedTagContent.ContentTag);
                    eventBuilder.Changed(Url.Parse(updatedTagContent.Url), attachedData);
                    await ExecuteAttachedDataDeletionPolicyOnContentChanged(transaction, attachedDataDataRows);
                }
            }
        }

        /// <summary>
        ///     Create the index of the file.
        /// </summary>
        /// <param name="url">The url of the file.</param>
        /// <param name="record">The record of the file. Null means the file is deleted.</param>
        private async ValueTask IndexFile(
            Url url,
            FileRecord? record,
            IDbTransaction transaction,
            FileChangeEventBuilder eventBuilder)
        {
            if (url.Path == "/")
            {
                return;
            }

            if (record == null)
            {
                await Delete(transaction, url, eventBuilder);
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
                    await Delete(transaction, url, eventBuilder);
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
                    await Delete(transaction, url, eventBuilder);
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
                    var attachedDataDataRows = await _fileTable.SelectAttachedDataByTargetAsync(transaction, oldFile.Id);
                    var attachedData = attachedDataDataRows
                        .Select(ConvertAttachedDataDataRowToAttachedData)
                        .ToImmutableArray();
                    await _fileTable.UpdateContentTagByIdAsync(
                        transaction,
                        oldFile.Id,
                        record.ContentTag);
                    eventBuilder.Changed(url, attachedData);
                    await ExecuteAttachedDataDeletionPolicyOnContentChanged(transaction, attachedDataDataRows);
                }
            }
        }

        private async ValueTask ExecuteAttachedDataDeletionPolicyOnContentChanged(
            IDbTransaction transaction,
            FileTable.AttachedDataDataRow[] fileAttachedDatas)
        {
            foreach (var fileAttachedData in fileAttachedDatas)
            {
                if (fileAttachedData.FileAttachedData.DeletionPolicy.HasFlag(FileAttachedData.DeletionPolicies.WhenFileContentChanged))
                {
                    await _fileTable.DeleteAttachedDataByIdAsync(transaction, fileAttachedData.Id);
                }
            }
        }

        private async ValueTask IndexAttachData(
            Url url,
            FileRecord fileRecord,
            FileAttachedData data,
            IDbTransaction transaction,
            FileChangeEventBuilder eventBuilder)
        {
            await IndexFile(url, fileRecord, transaction, eventBuilder);

            var file = await _fileTable.SelectByUrlAsync(transaction, url.ToString());

            await _fileTable.InsertOrReplaceAttachedDataAsync(transaction, file!.Id, data);
        }

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

                await Delete(transaction, currentUrl, eventBuilder);
            }

            for (i++; i <= pathPart.Length; i++)
            {
                var currentUrl = url with { Path = "/" + string.Join('/', pathPart.Take(i).ToArray()) };
                directoryId = await _fileTable.InsertAsync(transaction, currentUrl.ToString(), directoryId, true, null, null);
            }

            return directoryId!.Value;
        }

        private async ValueTask Delete(IDbTransaction transaction, Url url, FileChangeEventBuilder eventBuilder)
        {
            var file = await _fileTable.SelectByUrlAsync(transaction, url.ToString());
            if (file == null)
            {
                return;
            }

            var attachedDatas = (await _fileTable.SelectAttachedDataByTargetAsync(transaction, file.Id))
                .Select(ConvertAttachedDataDataRowToAttachedData).ToImmutableArray();

            var startsWith = url.ToString();
            if (!startsWith.EndsWith("/", StringComparison.Ordinal))
            {
                startsWith = startsWith + "/";
            }

            var childFiles = await _fileTable.SelectByStartsWithAsync(transaction, startsWith).ToArrayAsync();
            var childAttachedDatas =
                (await _fileTable.SelectAttachedDataByStartsWithAsync(transaction, startsWith))
                .GroupBy(row => row.Target)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ConvertAttachedDataDataRowToAttachedData).ToImmutableArray());

            await _fileTable.DeleteByUrlAsync(transaction, url.ToString());
            await _fileTable.DeleteByStartsWithAsync(transaction, startsWith);

            if (file.IdentifierTag != null)
            {
                eventBuilder.Deleted(Url.Parse(file.Url), attachedDatas);
            }

            foreach (var childFile in childFiles)
            {
                if (childFile.IdentifierTag != null)
                {
                    eventBuilder.Deleted(
                        Url.Parse(childFile.Url),
                        childAttachedDatas.GetValueOrDefault(childFile.Id, ImmutableArray.Create<FileAttachedData>()));
                }
            }
        }

        private async ValueTask EmitFileChangeEvent(FileEvent[] changeEvents)
        {
            if (changeEvents.Length == 0)
            {
                return;
            }

            await _fileChangeEventQueue.Writer.WriteAsync(changeEvents);
        }

        private FileAttachedData ConvertAttachedDataDataRowToAttachedData(FileTable.AttachedDataDataRow dataRow)
        {
            return dataRow.FileAttachedData;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _hintQueue.Writer.Complete();
                    _hintConsumerTask?.Unwrap().Wait();
                    _fileChangeEventQueue.Writer.Complete();
                    _fileChangeEventConsumerTask?.Unwrap().Wait();
                }

                _disposed = true;
            }
        }

        ~DatabaseHintFileTracker()
        {
            Dispose(false);
        }

        private class FileChangeEventBuilder
        {
            private readonly List<FileEvent> _events = new();

            public void Created(Url url)
            {
                _events.Add(
                    new FileEvent(Tracker.FileEvent.EventType.Created, url));
            }

            public void Changed(Url url, ImmutableArray<FileAttachedData> attachedData)
            {
                _events.Add(
                    new FileEvent(Tracker.FileEvent.EventType.Changed, url, attachedData));
            }

            public void Deleted(Url url, ImmutableArray<FileAttachedData> attachedData)
            {
                _events.Add(
                    new FileEvent(Tracker.FileEvent.EventType.Deleted, url, attachedData));
            }

            public FileEvent[] Build()
            {
                return _events.ToArray();
            }
        }

        private record AttachedResourceTagHint(
            Url Url,
            FileRecord FileRecord,
            FileAttachedData AttachedData) : Hint;
    }
}
