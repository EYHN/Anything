using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Mime;
using Anything.Preview.Thumbnails.Cache;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using SkiaSharp;
using NotSupportedException = Anything.FileSystem.Exception.NotSupportedException;

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

        private readonly IThumbnailsCacheStorage _thumbnailsCache;

        public ThumbnailsService(IFileService fileService, IMimeTypeService mimeType, IThumbnailsCacheStorage thumbnailsCache)
        {
            _fileService = fileService;
            _mimeType = mimeType;
            _thumbnailsCache = thumbnailsCache;
        }

        public async ValueTask<bool> IsSupportThumbnail(FileHandle fileHandle)
        {
            var stats = await _fileService.Stat(fileHandle);
            var mimeType = await _mimeType.GetMimeType(fileHandle, new MimeTypeOption());
            return _renderers.Any(renderer => renderer.IsSupported(new ThumbnailsRenderFileInfo(fileHandle, stats, mimeType)));
        }

        public async ValueTask<IThumbnail?> GetThumbnail(FileHandle fileHandle, ThumbnailOption option)
        {
            var targetSize = option.Size;
            var targetImageFormat = option.ImageFormat;
            if (targetImageFormat != "image/png")
            {
                throw new ArgumentException("Image format not support!");
            }

            var stats = await _fileService.Stat(fileHandle);

            var fileHash = stats.Hash;

            // Read the Cache
            var cachedThumbnails = await _thumbnailsCache.GetCache(fileHandle, fileHash);

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
                    using var encodedData = resizedBitmap.Encode(SKEncodedImageFormat.Png, 100);

                    var resizedThumbnail = new MemoryThumbnail(encodedData.ToArray(), "image/png", targetSize);
                    try
                    {
                        await _thumbnailsCache.Cache(fileHandle, fileHash, resizedThumbnail);
                    }
                    catch (NotSupportedException)
                    {
                        Console.WriteLine("File system not support cache.");
                    }

                    return resizedThumbnail;
                }
            }

            var mimeType = await _mimeType.GetMimeType(fileHandle, new MimeTypeOption());
            var fileInfo = new ThumbnailsRenderFileInfo(fileHandle, stats, mimeType);

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

                var encodedData = ctx.SnapshotPngBuffer();

                var thumbnail = new MemoryThumbnail(encodedData, "image/png", targetSize);
                try
                {
                    await _thumbnailsCache.Cache(fileHandle, fileHash, thumbnail);
                }
                catch (NotSupportedException)
                {
                    Console.WriteLine("File system not support cache.");
                }

                return thumbnail;
            }

            return null;
        }

        public void RegisterRenderer(IThumbnailsRenderer renderer)
        {
            _renderers.Add(renderer);
        }
    }
}
