using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using StagingBox.Utils;
using SkiaSharp;

namespace StagingBox.Preview.Icons
{
    public class StaticIconsService
    {
        private IconsCacheDatabase Db { get; }

        public StaticIconsService(string databasePath)
        {
            Db = new IconsCacheDatabase(databasePath);
            Db.Open().Wait();
        }

        public async Task<Stream?> GetIcon(string name, int size)
        {
            IconsCache? icons = await Db.GetIcons("icon:" + name);
            return icons != null ? await icons.GetIconData(size) : null;
        }

        public static async Task BuildCache(string databasePath, string iconsDirectory)
        {
            using IconsRenderContext ctx = new IconsRenderContext();
            using IconsCacheDatabase db = new IconsCacheDatabase(databasePath);

            await db.Open();

            foreach (var iconFile in Directory.EnumerateFiles(iconsDirectory))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                string? iconName = Path.GetFileNameWithoutExtension(iconFile);
                var iconFileInfo = new FileInfo(iconFile);

                IconsCache iconsCache = await db.GetOrAddOrUpdate(
                    "icon:" + iconName,
                    IconsDatabase.CalcFileEtag(iconFileInfo.LastWriteTimeUtc, iconFileInfo.Length)
                );

                var rendered = false;
                Action lazyRender = () =>
                {
                    if (rendered) return;
                    ctx.Resize(IconsConstants.MaxSize, IconsConstants.MaxSize, false);
                    string svgStr = System.IO.File.ReadAllText(iconFile);
                    RenderUtils.RenderSvg(ctx, svgStr, new SKPaint {BlendMode = SKBlendMode.Src});
                    rendered = true;
                };

                foreach (int size in IconsConstants.AvailableSize.OrderByDescending(size => size))
                    if (!iconsCache.HasSize(size))
                    {
                        lazyRender();
                        ctx.Resize(size, size);
                        using SKData? encoded = ctx.SnapshotPng();
                        await iconsCache.AddIcon(size, "image/png", encoded.AsStream());
                    }

                sw.Stop();
                TimeSpan ts2 = sw.Elapsed;

                if (rendered)
                    Console.WriteLine($"Build Icon Cache \"{iconName}\" Success! Takes {ts2.TotalMilliseconds}ms");
            }
        }
    }
}
