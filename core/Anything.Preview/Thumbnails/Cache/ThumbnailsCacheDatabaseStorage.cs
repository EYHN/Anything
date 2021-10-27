using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Fork;
using Anything.Utils;
using Microsoft.EntityFrameworkCore;

namespace Anything.Preview.Thumbnails.Cache
{
    public class ThumbnailsCacheDatabaseStorage : Disposable, IThumbnailsCacheStorage
    {
        private readonly EfCoreFileForkService _fileForkService;
        private readonly EfCoreFileForkService.MemoryStorage _fileForkServiceStorage;

        public ThumbnailsCacheDatabaseStorage(IFileService fileService)
        {
            _fileForkServiceStorage = new EfCoreFileForkService.MemoryStorage();
            _fileForkService = new EfCoreFileForkService(
                fileService,
                "thumbnails-cache",
                _fileForkServiceStorage,
                new[] { typeof(CachedThumbnail) });
        }

        public async ValueTask Cache(FileHandle fileHandle, FileHash fileHash, IThumbnail thumbnail)
        {
            await using var thumbnailStream = thumbnail.GetStream();
            await using var memoryStream = new MemoryStream((int)thumbnailStream.Length);
            await thumbnailStream.CopyToAsync(memoryStream);

            await using var fileForkContext = _fileForkService.CreateContext();
            var fileEntity = await fileForkContext.GetOrCreateFileEntity(fileHandle);
            var cacheThumbnails = fileForkContext.Set<CachedThumbnail>();
            cacheThumbnails
                .Add(new CachedThumbnail
                {
                    File = fileEntity,
                    Size = thumbnail.Size,
                    ImageFormat = thumbnail.ImageFormat,
                    Data = memoryStream.ToArray(),
                    FileHash = fileHash
                });
            cacheThumbnails
                .RemoveRange(cacheThumbnails.AsQueryable().Where(t => t.File == fileEntity && t.FileHash != fileHash));
            await fileForkContext.SaveChangesAsync();
        }

        public async ValueTask<IThumbnail[]> GetCache(FileHandle fileHandle, FileHash fileHash)
        {
            await using var fileForkContext = _fileForkService.CreateContext();
            var thumbnails = await fileForkContext.Set<CachedThumbnail>().AsQueryable()
                .Where(t => t.File.FileHandle == fileHandle && t.FileHash == fileHash)
                .Select(t => new { t.Id, t.Size, t.ImageFormat }).ToListAsync();

            return thumbnails.Select(t => new LazyThumbnail(this, t.Id, t.ImageFormat, t.Size) as IThumbnail).ToArray();
        }

        public async ValueTask<int> GetCount()
        {
            await using var fileForkContext = _fileForkService.CreateContext();
            return await fileForkContext.Set<CachedThumbnail>().AsQueryable().CountAsync();
        }

        private class LazyThumbnail : IThumbnail
        {
            private readonly ThumbnailsCacheDatabaseStorage _storage;
            private readonly int _id;

            public LazyThumbnail(ThumbnailsCacheDatabaseStorage storage, int id, string imageFormat, int size)
            {
                _storage = storage;
                _id = id;
                ImageFormat = imageFormat;
                Size = size;
            }

            public string ImageFormat { get; }

            public int Size { get; }

            public Stream GetStream()
            {
                using var context = _storage._fileForkService.CreateContext();
                var thumbnails = context.Set<CachedThumbnail>().AsQueryable().Single(t => t.Id == _id);
                return new MemoryStream(thumbnails.Data, false);
            }
        }

        private class CachedThumbnail : EfCoreFileForkService.FileForkEntity
        {
            public int Id { get; set; }

            public FileHash FileHash { get; set; } = null!;

            public string ImageFormat { get; set; } = null!;

            public int Size { get; set; }

            public byte[] Data { get; set; } = null!;
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileForkService.Dispose();
            _fileForkServiceStorage.Dispose();
        }
    }
}
