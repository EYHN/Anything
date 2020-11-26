using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.File;
using OwnHub.Preview.Icons.Renderers;
using OwnHub.Utils;
using SkiaSharp;

namespace OwnHub.Preview.Icons
{
    public class DynamicIconsService
    {
        public static readonly IDynamicIconsRenderer[] IconsRenderers =
        {
            new ImageFileRenderer(),
            new TextFileRenderer()
        };

        private IconsCacheDatabase CacheReadContext { get; }
        private ObjectPool<IconsCacheDatabase> CacheWriteContextPool { get; }
        private ObjectPool<IconsRenderContext> RenderContextPool { get; }
        private AsyncTaskWorker CacheWriteWorker { get; } = new AsyncTaskWorker(5);

        public DynamicIconsService(SqliteConnectionFactory connectionFactory)
        {
            RenderContextPool = new ObjectPool<IconsRenderContext>(Environment.ProcessorCount, () => new IconsRenderContext());
            
            var cacheWriteContext = new IconsCacheDatabase(connectionFactory.Make(SqliteOpenMode.ReadWriteCreate));
            cacheWriteContext.Open().Wait();
            
            CacheWriteContextPool = new ObjectPool<IconsCacheDatabase>(1);
            CacheWriteContextPool.Push(cacheWriteContext);
            
            CacheReadContext = new IconsCacheDatabase(connectionFactory.Make(SqliteOpenMode.ReadOnly));
            CacheReadContext.Open().Wait();
        }

        public IEnumerable<IDynamicIconsRenderer> MatchRenderers(IFile file)
        {
            return IconsRenderers.Where(renderer => renderer.IsSupported(file)).ToArray();
        }

        public bool IsSupported(IFile file)
        {
            return IconsRenderers.Any(renderer => renderer.IsSupported(file));
        }

        public async Task<Stream?> Render(IFile file, int targetSize)
        {
            // Get Cache Identifier
            string filePath = file.Path;

            // Clac Cache Etag
            IFileStats? stats = await file.Stats;
            string? etag = stats != null && stats.ModifyTime != null && stats.Size != null
                ? IconsDatabase.CalcFileEtag((DateTimeOffset) stats.ModifyTime, (long) stats.Size)
                : null;
            
            IEnumerable<IDynamicIconsRenderer> renderers = MatchRenderers(file);

            // Read the Cache
            IconsCache? cache = await CacheReadContext.GetIcons(filePath);
            if (cache != null) // If found the Cache
                if (cache.Etag == etag) // If Etag is Right
                {
                    if (cache.HasSize(targetSize)) // If the target size icon has been cached
                        return await cache.GetIconData(targetSize);

                    // If the target size icon not cached
                    // Find a larger size icon cache and compress to the target size
                    IEnumerable<int>? biggerSizes = IconsConstants.AvailableSize.Where(size => size > targetSize)
                        .OrderBy(size => size);

                    foreach (int biggerSize in biggerSizes)
                    {
                        Stream? encodedStream = await cache.GetIconData(biggerSize);
                        if (encodedStream != null)
                            using (SKBitmap bitmap = SKBitmap.Decode(encodedStream))
                            {
                                // Compress the icon to the target size
                                using (SKBitmap resizedBitmap = bitmap.Resize(new SKSizeI(targetSize, targetSize),
                                    SKFilterQuality.High))
                                {
                                    SKData encodedData = resizedBitmap.Encode(SKEncodedImageFormat.Png, 100);

                                    // Save Cache
                                    await CacheIconData(filePath, etag, targetSize, "image/png", encodedData.AsStream());

                                    return encodedData.AsStream();
                                }
                            }
                    }
                }


            using ObjectPool<IconsRenderContext>.Container? poolItem = await RenderContextPool.GetContainerAsync();
            
            IconsRenderContext ctx = poolItem.Value;

            ctx.Resize(targetSize, targetSize, false);

            foreach (var renderer in renderers)
            {
                var success = false;

                ctx.Save();
                try
                {
                    var renderInfo = new DynamicIconsRenderInfo(file);
                    success = await renderer.Render(ctx, renderInfo);
                }
                finally
                {
                    ctx.Restore();
                }

                if (!success) continue;
                if (ctx.Width != targetSize || ctx.Height != targetSize) ctx.Resize(targetSize, targetSize);

                SKData? encodedData = ctx.SnapshotPng();

                if (etag != null)
                {
                    // Cache EncodedStream
                    await CacheIconData(filePath, etag, targetSize, "image/png", encodedData.AsStream());
                }

                return encodedData.AsStream();
            }

            return null;
        }

        private async Task CacheIconData(string filePath, string etag, int size, string format, Stream data)
        {
            await CacheWriteWorker.Run(async () =>
            {
                using var cacheWriteContext = await CacheWriteContextPool.GetContainerAsync();

                IconsCache cache =
                    await cacheWriteContext.Value.GetOrAddOrUpdate(filePath, etag);
                await cache.AddIcon(size, format, data);
            });
        }
    }
}