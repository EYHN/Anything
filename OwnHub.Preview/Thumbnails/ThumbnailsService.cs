using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem;
using OwnHub.Preview.MimeType;
using OwnHub.Preview.Thumbnails.Cache;
using OwnHub.Preview.Thumbnails.Renderers;
using OwnHub.Utils;
using SkiaSharp;

namespace OwnHub.Preview.Thumbnails
{
    public class ThumbnailsService : IThumbnailsService
    {
        private readonly IMimeTypeService _mimeType;

        private readonly IThumbnailsCacheStorage _thumbnailsCache;

        private readonly IFileSystemService _fileSystem;

        private ObjectPool<ThumbnailsRenderContext> _renderContextPool =
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
            if (option.ImageFormat != "image/png")
            {
                throw new ArgumentException("Image format not support!");
            }

            var targetSize = option.Size;
            var stats = await _fileSystem.Stat(url);

            var fileRecord = stats.ToFileRecord();
            var tag = fileRecord.IdentifierTag + ":" + fileRecord.ContentTag;

            // Read the Cache
            // var cache = await _thumbnailsCache.GetCache(url, tag, GetCacheKey(targetSize, option.ImageFormat));
            //
            // if (cache != null)
            // {
            //     return new CachedThumbnail(cache, option.ImageFormat, targetSize);
            // }

            var mimeType = await _mimeType.GetMimeType(url, new MimeTypeOption());
            var thumbnailRenderOption = new ThumbnailsRenderOption(url) { FileType = stats.Type, MimeType = mimeType, Size = option.Size };

            using var poolItem = await _renderContextPool.GetRefAsync();

            var ctx = poolItem.Value;

            ctx.Resize(targetSize, targetSize, false);

            var matchedRenderers = _renderers.Where(renderer => renderer.IsSupported(thumbnailRenderOption));

            foreach (var renderer in matchedRenderers)
            {
                var success = false;

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

                return new SkiaThumbnail(encodedData, "image/png", targetSize);
            }

            return null;
        }
    }
}
