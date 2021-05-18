using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using StagingBox.FileSystem;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails.Cache;
using StagingBox.Preview.Thumbnails.Renderers;
using StagingBox.Utils;

namespace StagingBox.Preview.Thumbnails
{
    public class ThumbnailsService
        : IThumbnailsService
    {
        private readonly IMimeTypeService _mimeType;

        private readonly IThumbnailsCacheStorage _thumbnailsCache;

        private readonly IFileSystemService _fileSystem;

        private readonly ObjectPool<ThumbnailsRenderContext> _renderContextPool =
            new(Environment.ProcessorCount, () => new ThumbnailsRenderContext());

        private readonly List<IThumbnailsRenderer> _renderers = new();

        public ThumbnailsService(IFileSystemService fileSystem, IMimeTypeService mimeType, IThumbnailsCacheStorage thumbnailsCache)
        {
            _fileSystem = fileSystem;
            _mimeType = mimeType;
            _thumbnailsCache = thumbnailsCache;
        }

        public void RegisterRenderer(IThumbnailsRenderer renderer)
        {
            _renderers.Add(renderer);
        }

        public async ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option)
        {
            var targetSize = option.Size;
            var targetImageFormat = option.ImageFormat;
            if (targetImageFormat != "image/png")
            {
                throw new ArgumentException("Image format not support!");
            }

            var stats = await _fileSystem.Stat(url);

            var fileRecord = stats.ToFileRecord();
            var tag = fileRecord.IdentifierTag + ":" + fileRecord.ContentTag;

            // Read the Cache
            var cachedThumbnails = await _thumbnailsCache.GetCache(url, tag);

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
                    using SKBitmap bitmap = SKBitmap.Decode(biggerSize.GetStream());
                    using SKBitmap resizedBitmap = bitmap.Resize(new SKSizeI(targetSize, targetSize), SKFilterQuality.High);
                    SKData encodedData = resizedBitmap.Encode(SKEncodedImageFormat.Png, 100);

                    var resizedThumbnail = new SkiaThumbnail(encodedData, "image/png", targetSize);
                    await _thumbnailsCache.Cache(url, tag, resizedThumbnail);
                    return resizedThumbnail;
                }
            }

            var mimeType = await _mimeType.GetMimeType(url, new MimeTypeOption());
            var thumbnailRenderOption = new ThumbnailsRenderOption(url) { FileType = stats.Type, MimeType = mimeType, Size = option.Size };

            using var poolItem = await _renderContextPool.GetRefAsync();

            var ctx = poolItem.Value;

            ctx.Resize(targetSize, targetSize, false);

            var matchedRenderers = _renderers.Where(renderer => renderer.IsSupported(thumbnailRenderOption));

            foreach (var renderer in matchedRenderers)
            {
                bool success;

                ctx.Save();
                try
                {
                    success = await renderer.Render(ctx, thumbnailRenderOption);
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
                await _thumbnailsCache.Cache(url, tag, thumbnail);
                return thumbnail;
            }

            return null;
        }
    }
}
