using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.SubCar;
using Anything.FileSystem.Tracker;
using Anything.Preview.MimeType;
using Anything.Preview.Thumbnails.Cache;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using SkiaSharp;

namespace Anything.Preview.Thumbnails
{
    public class ThumbnailsService
        : IThumbnailsService
    {
        private readonly IFileService _fileService;

        private readonly IMimeTypeService _mimeType;

        private readonly ObjectPool<ThumbnailsRenderContext> _renderContextPool =
            new(Environment.ProcessorCount, () => new ThumbnailsRenderContext());

        private readonly List<IThumbnailsRenderer> _renderers = new();

        private readonly ThumbnailsCacheController _thumbnailsCacheController;

        public ThumbnailsService(IFileService fileService, IMimeTypeService mimeType, IThumbnailsCacheStorage thumbnailsCache)
        {
            _fileService = fileService;
            _mimeType = mimeType;
            _thumbnailsCacheController = new ThumbnailsCacheController(fileService, thumbnailsCache);
        }

        public async ValueTask<bool> IsSupportThumbnail(Url url)
        {
            var stats = await _fileService.Stat(url);
            var mimeType = await _mimeType.GetMimeType(url, new MimeTypeOption());
            return _renderers.Any(renderer => renderer.IsSupported(new ThumbnailsRenderFileInfo(url, stats, mimeType)));
        }

        public async ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option)
        {
            var targetSize = option.Size;
            var targetImageFormat = option.ImageFormat;
            if (targetImageFormat != "image/png")
            {
                throw new ArgumentException("Image format not support!");
            }

            var stats = await _fileService.Stat(url);

            var fileRecord = FileRecord.FromFileStats(stats);

            // Read the Cache
            var cachedThumbnails = await _thumbnailsCacheController.GetCache(url, fileRecord);

            if (cachedThumbnails.Length != 0)
            {
                var match = cachedThumbnails.FirstOrDefault(
                    thumbnail => thumbnail.Size == targetSize && thumbnail.ImageFormat == targetImageFormat);
                if (match != null)
                {
                    return match;
                }

                // If the target thumbnail icon not cached
                // Find a bigger size thumbnail cache and compress to the target size
                var biggerSize = cachedThumbnails
                    .Where(thumbnail => thumbnail.Size > targetSize && thumbnail.ImageFormat == targetImageFormat)
                    .OrderBy(thumbnail => thumbnail.Size).FirstOrDefault();
                if (biggerSize != null)
                {
                    using var bitmap = SKBitmap.Decode(biggerSize.GetStream());
                    using var resizedBitmap = bitmap.Resize(new SKSizeI(targetSize, targetSize), SKFilterQuality.High);
                    var encodedData = resizedBitmap.Encode(SKEncodedImageFormat.Png, 100);

                    var resizedThumbnail = new SkiaThumbnail(encodedData, "image/png", targetSize);
                    await _thumbnailsCacheController.Cache(url, fileRecord, resizedThumbnail);

                    return resizedThumbnail;
                }
            }

            var mimeType = await _mimeType.GetMimeType(url, new MimeTypeOption());
            var fileInfo = new ThumbnailsRenderFileInfo(url, stats, mimeType);

            using var poolItem = await _renderContextPool.GetRefAsync();

            var ctx = poolItem.Value;
            ctx.Resize(targetSize, targetSize, false);

            var matchedRenderers = _renderers.Where(renderer => renderer.IsSupported(fileInfo));

            var renderOption = new ThumbnailsRenderOption { Size = option.Size };
            foreach (var renderer in matchedRenderers)
            {
                bool success;

                ctx.Save();
                try
                {
                    success = await renderer.Render(ctx, fileInfo, renderOption);
                }
                finally
                {
                    ctx.Restore();
                }

                if (!success)
                {
                    continue;
                }

                if (ctx.Width != targetSize || ctx.Height != targetSize)
                {
                    ctx.Resize(targetSize, targetSize);
                }

                var encodedData = ctx.SnapshotPng();

                var thumbnail = new SkiaThumbnail(encodedData, "image/png", targetSize);
                await _thumbnailsCacheController.Cache(url, fileRecord, thumbnail);
                return thumbnail;
            }

            return null;
        }

        public void RegisterRenderer(IThumbnailsRenderer renderer)
        {
            _renderers.Add(renderer);
        }

        private record ThumbnailsCacheSubCar(long Id);

        private record ThumbnailsCacheParameter(Url Url, FileRecord FileRecord, IThumbnail Thumbnail);

        private class ThumbnailsCacheController : SubCarController<ThumbnailsCacheSubCar, ThumbnailsCacheParameter>
        {
            private readonly IThumbnailsCacheStorage _thumbnailsCache;

            public ThumbnailsCacheController(IFileService fileService, IThumbnailsCacheStorage thumbnailsCache)
                : base(fileService, FileAttachedData.DeletionPolicies.WhenFileContentChanged)
            {
                _thumbnailsCache = thumbnailsCache;
            }

            public override string Name => "T";

            protected override async Task<ThumbnailsCacheSubCar[]> Create(ThumbnailsCacheParameter[] parameters)
            {
                var subCars = new List<ThumbnailsCacheSubCar>();
                foreach (var (url, fileRecord, thumbnail) in parameters)
                {
                    var id = await _thumbnailsCache.Cache(url, fileRecord, thumbnail);
                    subCars.Add(new ThumbnailsCacheSubCar(id));
                }

                return subCars.ToArray();
            }

            protected override Task Delete(ThumbnailsCacheSubCar[] entries)
            {
                return _thumbnailsCache.DeleteBatch(entries.Select(entry => entry.Id).ToArray()).AsTask();
            }

            protected override string Serialize(ThumbnailsCacheSubCar entry)
            {
                return entry.Id.ToString();
            }

            protected override ThumbnailsCacheSubCar Deserialize(string payload)
            {
                return new(Convert.ToInt32(payload));
            }

            public ValueTask<IThumbnail[]> GetCache(Url url, FileRecord fileRecord)
            {
                return _thumbnailsCache.GetCache(url, fileRecord);
            }

            public Task Cache(Url url, FileRecord fileRecord, IThumbnail thumbnail)
            {
                return Attach(url, fileRecord, new ThumbnailsCacheParameter(url, fileRecord, thumbnail));
            }
        }
    }
}
