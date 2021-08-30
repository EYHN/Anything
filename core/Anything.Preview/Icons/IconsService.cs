using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;
using SkiaSharp;

namespace Anything.Preview.Icons
{
    public class IconsService : IIconsService
    {
        private readonly Dictionary<(string Name, int Size, string Format), MemoryIconImage> _cached = new();

        private readonly IFileService _fileService;

        public IconsService(IFileService fileService)
        {
            _fileService = fileService;
            BuildCache();
        }

        public async ValueTask<string> GetIconId(Url url)
        {
            var stats = await _fileService.Stat(url);
            string targetIconId;
            if (stats.Type.HasFlag(FileType.File))
            {
                targetIconId = "regular_file";
            }
            else if (stats.Type.HasFlag(FileType.Directory))
            {
                targetIconId = "directory";
            }
            else
            {
                targetIconId = "unknown_file";
            }

            return targetIconId;
        }

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option)
        {
            if (option.ImageFormat != "image/png")
            {
                throw new NotSupportedException();
            }

            if (_cached.TryGetValue((id, option.Size, option.ImageFormat), out var icon))
            {
                return ValueTask.FromResult(icon as IIconImage);
            }

            throw new NotSupportedException();
        }

        private void BuildCache()
        {
            using IconsRenderContext ctx = new();
            foreach (var name in new[] { "regular_file", "directory", "unknown_file" })
            {
                var buffer = Resources.ReadEmbeddedFile(typeof(IconsService).Assembly, $"/Resources/Icons/{name}.svg");
                var svgStr = Encoding.UTF8.GetString(buffer);

                ctx.Resize(IconsConstants.MaxSize, IconsConstants.MaxSize, false);
                using var paint = new SKPaint { BlendMode = SKBlendMode.Src };
                RenderUtils.RenderSvg(ctx, svgStr, paint);

                foreach (var size in IconsConstants.AvailableSize.OrderByDescending(size => size))
                {
                    ctx.Resize(size, size);
                    using var encoded = ctx.SnapshotPng();
                    _cached.Add((name, size, "image/png"), new MemoryIconImage(encoded.ToArray(), "image/png", size));
                }
            }
        }
    }
}
