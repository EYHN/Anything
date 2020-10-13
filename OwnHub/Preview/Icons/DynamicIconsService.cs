using OwnHub.File;
using OwnHub.Preview.Icons.Renderers;
using OwnHub.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Icons
{
    public class DynamicIconsService
    {
        public ObjectPool<IconsRenderContext> RenderContextPool;
        public IconsDatabase CacheDatabase;
        public static IDynamicIconsRenderer[] IconsRenderers = new IDynamicIconsRenderer[] {
            new ImageFileRenderer(),
            new TextFileRenderer()
        };

        public DynamicIconsService()
        {
            RenderContextPool = new ObjectPool<IconsRenderContext>(Environment.ProcessorCount, CreateRenderContext);
        }

        public IconsRenderContext CreateRenderContext()
        {
            return new IconsRenderContext();
        }

        public IDynamicIconsRenderer[] MatchRenderers(IFile file)
        {
            return IconsRenderers.Where((renderer) =>
            {
                return renderer.IsSupported(file);
            }).ToArray();
        }

        public bool IsSupported(IFile file)
        {
            return IconsRenderers.Any((Renderer) => Renderer.IsSupported(file));
        }

        public async Task<Stream> Render(IFile file, int TargetSize)
        {
            IDynamicIconsRenderer[] renderers = MatchRenderers(file);

            // Get Cache Identifier
            string Identifier = "dynamic-icon:" + file.Path;

            // Clac Cache Etag
            IFileStats Stats = await file.Stats;
            string Etag = IconsDatabase.CalcFileEtag((DateTimeOffset)Stats.ModifyTime, (long)Stats.Size);

            // Read the Cache
            if (CacheDatabase != null)
            {
                var CacheEntity = await CacheDatabase.Read(Identifier);
                if (CacheEntity != null)// If found the Cache
                {
                    if (CacheEntity.Etag == Etag) // If Etag is Right
                    {
                        if (CacheEntity.Has(TargetSize)) // If the target size icon has been cached
                        {
                            return CacheEntity.Read(TargetSize);
                        }
                        else
                        {
                            // If the target size icon not cached
                            // Find a larger size icon cache and compress to the target size
                            var BiggerSizes = IconsConstants.AvailableSize.Where(Size => Size > TargetSize);
                            BiggerSizes.OrderBy(Size => Size);

                            foreach (int Size in BiggerSizes)
                            {
                                if (CacheEntity.Has(Size)) // If this size icon has been cached
                                {
                                    Stream EncodedStream = CacheEntity.Read(Size);
                                    using (SKBitmap bitmap = SKBitmap.Decode(EncodedStream))
                                    {
                                        // Compress the icon to the target size
                                        using (SKBitmap ResizedBitmap = bitmap.Resize(new SKSizeI(TargetSize, TargetSize), SKFilterQuality.High))
                                        {
                                            SKData EncodedData = ResizedBitmap.Encode(SKEncodedImageFormat.Png, 100);

                                            // Save Cache
                                            _ = CacheEntity.Write(TargetSize, EncodedData.AsStream());

                                            return EncodedData.AsStream();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            using (var PoolItem = await RenderContextPool.GetDisposableAsync())
            {
                IconsRenderContext ctx = PoolItem.Item;

                ctx.Resize(TargetSize, TargetSize, false);

                foreach (var renderer in renderers)
                {
                    bool success = false;

                    ctx.Save();
                    try
                    {
                        var renderInfo = new DynamicIconsRenderInfo()
                        {
                            file = file
                        };
                        success = await renderer.Render(ctx, renderInfo);
                    }
                    finally
                    {
                        ctx.Restore();
                    }

                    if (success)
                    {
                        if (ctx.Width != TargetSize || ctx.Height != TargetSize)
                        {
                            ctx.Resize(TargetSize, TargetSize, zoomContent: true);
                        }

                        var EncodedData = ctx.SnapshotPNG();

                        if (CacheDatabase != null)
                        {
                            // Cache EncodedStream
                            _ = Task.Run(async () =>
                            {
                                var Entity = await CacheDatabase.OpenOrCreateOrUpdate(Identifier, Etag);
                                await Entity.Write(TargetSize, EncodedData.AsStream());
                            });
                            
                        }

                        return EncodedData.AsStream();
                    }
                }
            }

            return null;
        }
    }
}
