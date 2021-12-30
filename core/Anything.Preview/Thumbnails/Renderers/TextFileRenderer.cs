using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;
using SkiaSharp;
using Topten.RichTextKit;

namespace Anything.Preview.Thumbnails.Renderers;

/// <summary>
///     Thumbnail renderer for text file.
/// </summary>
public class TextFileRenderer : IThumbnailsRenderer
{
    private static readonly Regex _svgPathRegex = new(
        "(?<=(<path[\\s\\S\\n]*d=\"))((?!\")[\\s\\S\\n])*",
        RegexOptions.ECMAScript | RegexOptions.Multiline);

    private static readonly Regex _svgFillRegex = new(
        "(?<=(<path[\\s\\S\\n]*fill=\"))((?!\")[\\s\\S\\n])*",
        RegexOptions.ECMAScript | RegexOptions.Multiline);

    private static readonly Dictionary<string, SvgPath> _fileLogos = ReadFileLogos();

    private static SKBitmap? _cachedBackground;

    private static readonly SKPath _clipPath = ParseSvgStr(Resources.ReadEmbeddedTextFile(
        typeof(TextFileRenderer).Assembly,
        "/Shared/design/generated/thumbnails/text/mask.svg")).Path;

    private readonly IFileService _fileService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextFileRenderer" /> class.
    /// </summary>
    /// <param name="fileService">The file system service.</param>
    public TextFileRenderer(IFileService fileService)
    {
        _fileService = fileService;
    }

    public virtual bool IsSupported(ThumbnailsRenderFileInfo fileInfo)
    {
        if (fileInfo.Type.HasFlag(FileType.File) && fileInfo.MimeType != null &&
            fileInfo.MimeType.Mime.StartsWith("text/", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    public async Task<bool> Render(
        ThumbnailsRenderContext ctx,
        ThumbnailsRenderFileInfo fileInfo,
        ThumbnailsRenderOption option)
    {
        var data = new byte[1024 * 8];
        var length = await _fileService.ReadFileStream(fileInfo.FileHandle, async stream => await stream.ReadAsync(data));
        var text = Encoding.UTF8.GetString(data, 0, length);
        text = text.Replace("\r\n", "\n", StringComparison.Ordinal);

        _cachedBackground ??= SKBitmap.Decode(ReadBackground());

        int targetWidth = ctx.Width, targetHeight = ctx.Height;
        ctx.Resize(ThumbnailsConstants.MaxSize, ThumbnailsConstants.MaxSize, false);

        using (new SKAutoCanvasRestore(ctx.Canvas))
        {
            using (var paint = new SKPaint { BlendMode = SKBlendMode.Src })
            {
                ctx.Canvas.DrawBitmap(_cachedBackground, SKRect.Create(0, 0, 128, 128), paint);
            }

            ctx.Canvas.ClipPath(_clipPath);

            float padding = 8;
            var tb = new Font.TextBlock();
            var maxWidth = 76f - (2 * padding);
            var maxHeight = 103.96f - (2 * padding);

            tb.MaxWidth = maxWidth;
            tb.MaxHeight = maxHeight;

            var styleNormal = new Style { TextColor = new SKColor(0, 0, 0), FontSize = 1.25f };

            tb.AddText(text, styleNormal);

            var paintOptions = new TextPaintOptions { Edging = SKFontEdging.Antialias };

            tb.Paint(ctx.Canvas, new SKPoint((128f - maxWidth) / 2, (128f - maxHeight) / 2), paintOptions);

            var fileName = await _fileService.GetFileName(fileInfo.FileHandle);

            SvgPath? logo;
            if (_fileLogos.TryGetValue(string.Concat("extname/", PathLib.Extname(fileName).AsSpan(1)), out logo) ||
                _fileLogos.TryGetValue(fileInfo.MimeType?.Mime ?? "", out logo))
            {
                using var fillPaint = new SKPaint { Color = logo.Fill };
                using var strokePaint = new SKPaint { Color = new SKColor(255, 255, 255), Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
                ctx.Canvas.DrawPath(logo.Path, strokePaint);
                ctx.Canvas.DrawPath(logo.Path, fillPaint);
            }
        }

        return true;
    }

    private static byte[] ReadBackground()
    {
        return Resources.ReadEmbeddedFile(typeof(TextFileRenderer).Assembly, "/Shared/design/generated/thumbnails/text/background.png");
    }

    private static Dictionary<string, SvgPath> ReadFileLogos()
    {
        var assembly = typeof(TextFileRenderer).Assembly;
        var decorationFilePath = new List<string>();
        decorationFilePath.AddRange(Resources.GetAllEmbeddedFile(assembly, "/Shared/design/generated/thumbnails/text/extname"));
        decorationFilePath.AddRange(Resources.GetAllEmbeddedFile(assembly, "/Shared/design/generated/thumbnails/text/text"));
        decorationFilePath.AddRange(Resources.GetAllEmbeddedFile(assembly, "/Shared/design/generated/thumbnails/text/application"));

        var decorationSvg = decorationFilePath.Select(path => (
            name: string.Join("/", path.Split('.')[^3..^1]),
            svgStr: Resources.ReadEmbeddedTextFile(assembly, path)));

        return decorationSvg.ToDictionary(
            item => item.name,
            item =>
                ParseSvgStr(item.svgStr));
    }

    private static SvgPath ParseSvgStr(string svgStr)
    {
        var path = _svgPathRegex.Match(svgStr);
        var fill = _svgFillRegex.Match(svgStr);
        if (!path.Success || !fill.Success)
        {
            throw new InvalidOperationException("parse svg failed");
        }

        return new SvgPath(
            SKPath.ParseSvgPathData(path.Value),
            SKColor.Parse(fill.Value));
    }

    public record SvgPath(SKPath Path, SKColor Fill);
}
