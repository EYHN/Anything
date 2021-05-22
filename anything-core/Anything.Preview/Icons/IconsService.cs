using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;
using SkiaSharp;

namespace Anything.Preview.Icons
{
    public class IconsService : IIconsService
    {
        private readonly Dictionary<(string Name, int Size, string Format), MemoryIcon> _cached = new();

        private readonly IFileSystemService _fileSystem;

        public IconsService(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void BuildCache()
        {
            using IconsRenderContext ctx = new IconsRenderContext();
            foreach (var name in new[] { "regular_file", "directory", "unknown_file" })
            {
                var stream = Resources.ReadEmbeddedFile(typeof(IconsService).Assembly, $"/Resources/Icons/{name}.svg");
                var streamReader = new StreamReader(stream);
                var svgStr = streamReader.ReadToEnd();

                ctx.Resize(IconsConstants.MaxSize, IconsConstants.MaxSize, false);
                RenderUtils.RenderSvg(ctx, svgStr, new SKPaint { BlendMode = SKBlendMode.Src });

                foreach (var size in IconsConstants.AvailableSize.OrderByDescending(size => size))
                {
                    ctx.Resize(size, size);
                    var encoded = ctx.SnapshotPng();
                    _cached.Add((name, size, "image/png"), new MemoryIcon(encoded.ToArray(), "image/png", size));
                }
            }
        }

        public async ValueTask<IIcon> GetIcon(Url url, IconsOption option)
        {
            if (option.ImageFormat != "image/png")
            {
                throw new NotSupportedException();
            }

            var stats = await _fileSystem.Stat(url);
            string targetIconName;
            if (stats.Type.HasFlag(FileType.File))
            {
                targetIconName = "regular_file";
            }
            else if (stats.Type.HasFlag(FileType.Directory))
            {
                targetIconName = "directory";
            }
            else
            {
                targetIconName = "unknown_file";
            }

            if (_cached.TryGetValue((targetIconName, option.Size, option.ImageFormat), out var icon))
            {
                return icon;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
