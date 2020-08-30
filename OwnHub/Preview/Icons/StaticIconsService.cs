using OwnHub.Preview.Icons.Renderers;
using MoreLinq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Icons
{
    public class StaticIconsService
    {
        public StaticIconsService(IconsDatabase db)
        {

        }

        public static async Task BuildCache(string databasePath, string iconsDirectory)
        {
            using (IconsRenderContext ctx = new IconsRenderContext())
            using (IconsDatabase db = new IconsDatabase(databasePath))
            {
                await db.Open();
                foreach (var iconFile in System.IO.Directory.EnumerateFiles(iconsDirectory))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    var iconName = Path.GetFileNameWithoutExtension(iconFile);
                    var iconFileInfo = new FileInfo(iconFile);

                    var iconEntity = await db.OpenOrCreateOrUpdate(
                        "icon:" + iconName,
                        IconsDatabase.CalcFileEtag(iconFileInfo.LastWriteTimeUtc, iconFileInfo.Length)
                        );

                    bool rendered = false;
                    Action lazyRender = () =>
                    {
                        if (rendered) return;
                        ctx.Resize(IconsConstants.MaxSize, IconsConstants.MaxSize, zoomContent: false);
                        string svgStr = System.IO.File.ReadAllText(iconFile);
                        RenderUtils.RenderSvg(ctx, svgStr, new SKPaint() { BlendMode = SKBlendMode.Src });
                        rendered = true;
                    };

                    foreach (var Size in IconsConstants.AvailableSize.OrderByDescending(Size => Size))
                    {
                        if (!iconEntity.Has(Size))
                        {
                            lazyRender();
                            ctx.Resize(Size, Size, zoomContent: true);
                            using (var encoded = ctx.SnapshotPNG())
                                await iconEntity.Write(Size, encoded.AsStream());
                        }
                    }

                    sw.Stop();
                    TimeSpan ts2 = sw.Elapsed;

                    if (rendered)
                    Console.WriteLine($"Build Icon Cache \"{ iconName }\" Success! Takes {ts2.TotalMilliseconds}ms");
                }
            }
        }
    }
}
