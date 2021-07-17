using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Preview.Thumbnails.Cache
{
    public class ThumbnailsCacheDatabaseStorage : IThumbnailsCacheStorage
    {
        private readonly SqliteContext _context;
        private readonly ThumbnailsCacheDatabaseStorageTable _thumbnailsCacheDatabaseStorageTable;

        public ThumbnailsCacheDatabaseStorage(SqliteContext context)
        {
            _thumbnailsCacheDatabaseStorageTable = new ThumbnailsCacheDatabaseStorageTable("IconsCache");
            _context = context;
            Create().AsTask().Wait();
        }

        public async ValueTask<long> Cache(Url url, FileRecord fileRecord, IThumbnail thumbnail)
        {
            await using var thumbnailStream = thumbnail.GetStream();
            await using var memoryStream = new MemoryStream((int)thumbnailStream.Length);
            await thumbnailStream.CopyToAsync(memoryStream);

            await using var transaction = _context.StartTransaction(ITransaction.TransactionMode.Mutation);
            var id = await _thumbnailsCacheDatabaseStorageTable.InsertOrReplaceAsync(
                transaction,
                url.ToString(),
                thumbnail.Size + ":" + thumbnail.ImageFormat,
                fileRecord.IdentifierTag + ":" + fileRecord.ContentTag,
                memoryStream.ToArray());
            await transaction.CommitAsync();
            return id;
        }

        public async ValueTask<IThumbnail[]> GetCache(Url url, FileRecord fileRecord)
        {
            await using var transaction = _context.StartTransaction(ITransaction.TransactionMode.Query);
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
            await using var transaction = _context.StartTransaction(ITransaction.TransactionMode.Mutation);
            await _thumbnailsCacheDatabaseStorageTable.DeleteAsync(
                transaction,
                id);
            await transaction.CommitAsync();
        }

        public async ValueTask DeleteBatch(long[] ids)
        {
            await using var transaction = _context.StartTransaction(ITransaction.TransactionMode.Mutation);
            foreach (var id in ids)
            {
                await _thumbnailsCacheDatabaseStorageTable.DeleteAsync(
                    transaction,
                    id);
            }

            await transaction.CommitAsync();
        }

        /// <summary>
        ///     Create database table.
        /// </summary>
        public async ValueTask Create()
        {
            await using var transaction = _context.StartTransaction(ITransaction.TransactionMode.Create);
            await _thumbnailsCacheDatabaseStorageTable.CreateAsync(transaction);
            await transaction.CommitAsync();
        }

        public async ValueTask<long> GetCount()
        {
            await using var transaction = _context.StartTransaction(ITransaction.TransactionMode.Query);
            return await _thumbnailsCacheDatabaseStorageTable.GetCount(transaction);
        }

        private byte[] GetData(long rowId)
        {
            using var transaction = _context.StartTransaction(ITransaction.TransactionMode.Query);
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
