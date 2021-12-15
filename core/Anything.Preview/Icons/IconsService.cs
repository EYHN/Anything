using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Anything.Preview.Icons;

using CacheDictionary = Dictionary<(string Name, int Size, string Format), IconImage>;

public class IconsService : IIconsService
{
    private static readonly Lazy<CacheDictionary> _cached = new(BuildCache);

    private readonly IFileService _fileService;

    private readonly ILogger<IconsService> _logger;

    public IconsService(IFileService fileService, ILogger<IconsService> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public async ValueTask<string> GetIconId(FileHandle fileHandle)
    {
        var stats = await _fileService.Stat(fileHandle);
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

    public ValueTask<IconImage> GetIconImage(string id, IconImageOption option)
    {
        if (option.ImageFormat != "image/png")
        {
            throw new NotSupportedException();
        }

        if (_cached.Value.TryGetValue((id, option.Size, option.ImageFormat), out var icon))
        {
            return ValueTask.FromResult(icon);
        }

        throw new NotSupportedException();
    }

    private static CacheDictionary BuildCache()
    {
        using IconsRenderContext ctx = new();
        var cached = new CacheDictionary();
        foreach (var name in new[] { "regular_file", "directory", "unknown_file" })
        {
            var buffer = Resources.ReadEmbeddedFile(typeof(IconsService).Assembly, $"/Shared/design/generated/icons/{name}.svg");
            var svgStr = Encoding.UTF8.GetString(buffer);

            ctx.Resize(IconsConstants.MaxSize, IconsConstants.MaxSize, false);
            using var paint = new SKPaint { BlendMode = SKBlendMode.Src };
            RenderUtils.RenderSvg(ctx, svgStr, paint);

            foreach (var size in IconsConstants.AvailableSize.OrderByDescending(size => size))
            {
                ctx.Resize(size, size);
                var encoded = ctx.SnapshotPngBuffer();
                cached.Add((name, size, "image/png"), new IconImage("image/png", size, encoded));
            }
        }

        return cached;
    }
}
