using System.Threading.Tasks;
using Nito.AsyncEx;
using OwnHub.Database;
using OwnHub.FileSystem;
using OwnHub.Utils;
using OwnHub.Utils.Event;

namespace OwnHub.Preview
{
    public class IconsCacheDatabaseStorage
    {
        private readonly IconsCacheDatabaseStorageTable _iconsCacheDatabaseStorageTable;

        private readonly SqliteContext _context;

        private readonly AsyncLock _writeLock = new();

        private readonly EventEmitter<Url> _beforeCacheEventEmitter = new();

        public Event<Url> OnBeforeCache => _beforeCacheEventEmitter.Event;

        public IconsCacheDatabaseStorage(SqliteContext context)
        {
            // FileSystemService = fileSystemService;
            // FileSystemService.OnFileChange += HandleFileChange;

            _iconsCacheDatabaseStorageTable = new IconsCacheDatabaseStorageTable("IconsCache");
            _context = context;
        }

        /// <summary>
        /// Create database table.
        /// </summary>
        public async ValueTask Create()
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Create);
            await _iconsCacheDatabaseStorageTable.CreateAsync(transaction);
            await transaction.CommitAsync();
        }

        // public void HandleFileChange(FileChangeEvent[] events)
        // {
        //     var deleteList = new List<Url>();
        //     foreach (var @event in events)
        //     {
        //         if (@event.Type is FileChangeEvent.EventType.Changed or FileChangeEvent.EventType.Deleted &&
        //             @event.Metadata.Any(metadata => metadata.Key == MetadataKey))
        //         {
        //             deleteList.Add(@event.Url);
        //         }
        //     }
        //
        //     Task.Run(() => DeleteBatch(deleteList.ToArray()));
        // }

        public async ValueTask Cache(Url url, FileRecord @record, string key, byte[] data)
        {
            using (await _writeLock.LockAsync())
            {
                await _beforeCacheEventEmitter.EmitAsync(url);
                // await FileSystemService.AttachMetadata(url, new FileMetadata(MetadataKey));
                await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
                await _iconsCacheDatabaseStorageTable.InsertOrReplaceAsync(
                    transaction,
                    url.ToString(),
                    key,
                    @record.IdentifierTag + ":" + @record.ContentTag,
                    data);
                await transaction.CommitAsync();
            }
        }

        public async ValueTask<byte[]?> GetCache(Url url, FileRecord @record, string key)
        {
            await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query);
            return await _iconsCacheDatabaseStorageTable.SelectAsync(
                transaction,
                url.ToString(),
                key,
                @record.IdentifierTag + ":" + @record.ContentTag);
        }

        public async ValueTask Delete(Url url)
        {
            using (await _writeLock.LockAsync())
            {
                await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
                await _iconsCacheDatabaseStorageTable.DeleteByPathAsync(transaction, url.ToString());
                await transaction.CommitAsync();
            }
        }

        public async ValueTask DeleteBatch(Url[] urls)
        {
            using (await _writeLock.LockAsync())
            {
                await using var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Mutation);
                foreach (var url in urls)
                {
                    await _iconsCacheDatabaseStorageTable.DeleteByPathAsync(transaction, url.ToString());
                }

                await transaction.CommitAsync();
            }
        }
    }
}
