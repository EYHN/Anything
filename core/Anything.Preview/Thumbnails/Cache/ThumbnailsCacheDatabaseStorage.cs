using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem;
using Anything.Utils;
using Anything.Utils.Event;
using Nito.AsyncEx;

namespace Anything.Preview.Thumbnails.Cache
{
    public class ThumbnailsCacheDatabaseStorage : IThumbnailsCacheStorage
    {
        private readonly EventEmitter<Url> _beforeCacheEventEmitter = new();

        private readonly SqliteContext _context;
        private readonly ThumbnailsCacheDatabaseStorageTable _thumbnailsCacheDatabaseStorageTable;

        private readonly AsyncLock _writeLock = new();

        public ThumbnailsCacheDatabaseStorage(SqliteContext context)
        {
            _thumbnailsCacheDatabaseStorageTable = new ThumbnailsCacheDatabaseStorageTable("IconsCache");
            _context = context;
            Create().AsTask().Wait();
        }

        public Event<Url> OnBeforeCache => _beforeCacheEventEmitter.Event;

        public async ValueTask<long> Cache(Url url, FileRecord fileRecord, IThumbnail thumbnail)
        {
            await using var thumbnailStream = thumbnail.GetStream();
            await using var memoryStream = new MemoryStream((int)thumbnailStream.Length);
            await thumbnailStream.CopyToAsync(memoryStream);

            using (await _writeLock.LockAsync())
            {
                await _beforeCacheEventEmitter.EmitAsync(url);
                await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
                var id = await _thumbnailsCacheDatabaseStorageTable.InsertOrReplaceAsync(
                    transaction,
                    url.ToString(),
                    thumbnail.Size + ":" + thumbnail.ImageFormat,
                    fileRecord.IdentifierTag + ":" + fileRecord.ContentTag,
                    memoryStream.ToArray());
                await transaction.CommitAsync();
                return id;
            }
        }

        public async ValueTask<IThumbnail[]> GetCache(Url url, FileRecord fileRecord)
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query);
            var dataRows = await _thumbnailsCacheDatabaseStorageTable.SelectAsync(
                transaction,
                url.ToString(),
                fileRecord.IdentifierTag + ":" + fileRecord.ContentTag);
            return dataRows.Select(
                row =>
                {
                    var split = row.Key.Split(':', 2);
                    var size = Convert.ToInt32(split[0]);
                    var format = split[1];
                    return new CachedThumbnail(this, row.Id, format, size) as IThumbnail;
                }).ToArray();
        }

        public async ValueTask Delete(long id)
        {
            using (await _writeLock.LockAsync())
            {
                await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
                await _thumbnailsCacheDatabaseStorageTable.DeleteAsync(
                    transaction,
                    id);
                await transaction.CommitAsync();
            }
        }

        public async ValueTask DeleteBatch(long[] ids)
        {
            using (await _writeLock.LockAsync())
            {
                await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
                foreach (var id in ids)
                {
                    await _thumbnailsCacheDatabaseStorageTable.DeleteAsync(
                        transaction,
                        id);
                }

                await transaction.CommitAsync();
            }
        }

        /// <summary>
        ///     Create database table.
        /// </summary>
        public async ValueTask Create()
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Create);
            await _thumbnailsCacheDatabaseStorageTable.CreateAsync(transaction);
            await transaction.CommitAsync();
        }

        public async ValueTask<long> GetCount()
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query);
            return await _thumbnailsCacheDatabaseStorageTable.GetCount(transaction);
        }

        private byte[] GetData(long rowId)
        {
            using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query);
            return _thumbnailsCacheDatabaseStorageTable.GetData(transaction, rowId);
        }

        private class CachedThumbnail : IThumbnail
        {
            private readonly long _rowId;
            private readonly ThumbnailsCacheDatabaseStorage _thumbnailsCacheDatabaseStorage;

            public CachedThumbnail(ThumbnailsCacheDatabaseStorage thumbnailsCacheDatabaseStorage, long rowId, string imageFormat, int size)
            {
                _thumbnailsCacheDatabaseStorage = thumbnailsCacheDatabaseStorage;
                _rowId = rowId;
                ImageFormat = imageFormat;
                Size = size;
            }

            public string ImageFormat { get; }

            public int Size { get; }

            public Stream GetStream()
            {
                var data = _thumbnailsCacheDatabaseStorage.GetData(_rowId);
                return new MemoryStream(data, false);
            }
        }
    }
}
