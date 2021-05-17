using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OwnHub.FileSystem;
using OwnHub.Utils;
using SkiaSharp;
using Topten.RichTextKit;

namespace OwnHub.Preview.Thumbnails.Renderers
{
    /// <summary>
    /// Thumbnail renderer for text file.
    /// </summary>
    public class TextFileRenderer : BaseThumbnailsRenderer
    {
        private readonly IFileSystemService _fileSystem;

        /// <inheritdoc/>
        protected override string[] SupportMimeTypes { get; } =
            Resources.ReadEmbeddedJsonFile<string[]>(
                typeof(TextFileRenderer).Assembly,
                "/Resources/Data/TextFileRenderer/SupportMimeType.json");

        private static readonly Dictionary<string, FileLogo> _fileLogos = ReadFileLogos();

        private static SKBitmap? _cachedBackground;

        private static readonly SKPath _clipPath = SKPath.ParseSvgPathData(
            "M72.9973 12.5L30.0318 12.5C28.0874 12.5 26.5 14.0988 26.5 16.0849V111.875C26.5 113.861 28.0874 115.46 30.0318 115.46H97.9682C99.9126 115.46 101.5 113.861 101.5 111.875V41.0027C101.5 39.932 101.031 38.9715 100.286 38.3139C99.654 37.7558 98.8246 37.4178 97.9151 37.4178H81.1671C78.6349 37.4178 76.5822 35.3651 76.5822 32.8329V16.0849C76.5822 15.0515 76.2433 13.9624 75.6429 13.35C75.0418 12.7369 74.0906 12.5 72.9973 12.5Z");

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFileRenderer"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system service.</param>
        public TextFileRenderer(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <inheritdoc/>
        public override async Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderOption option)
        {
            var stream = await _fileSystem.OpenReadFileStream(option.Url);
            var data = new byte[1024 * 8];
            await stream.ReadAsync(data);
            string text = Encoding.UTF8.GetString(data);

            _cachedBackground ??= SKBitmap.Decode(ReadBackgroundStream());

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

                var paintOptions = new TextPaintOptions { IsAntialias = false };

                tb.Paint(ctx.Canvas, new SKPoint((128f - maxWidth) / 2, (128f - maxHeight) / 2), paintOptions);

                FileLogo logo;
                if (_fileLogos.TryGetValue("extname/" + PathLib.Extname(option.Url.Path).Substring(1), out logo) ||
                    _fileLogos.TryGetValue(option.MimeType ?? "", out logo))
                {
                    using (var fillPaint = new SKPaint { Color = logo.Fill })
                    using (var strokePaint = new SKPaint
                    {
                        Color = new SKColor(255, 255, 255), Style = SKPaintStyle.Stroke, StrokeWidth = 2
                    })
                    {
                        ctx.Canvas.DrawPath(logo.Path, strokePaint);
                        ctx.Canvas.DrawPath(logo.Path, fillPaint);
                    }
                }
            }

            ctx.Resize(targetWidth, targetHeight);

            return true;
        }

        private static Stream ReadBackgroundStream()
        {
            return Resources.ReadEmbeddedFile(typeof(TextFileRenderer).Assembly, "/Resources/Data/TextFileRenderer/File.png");
        }

        public static Dictionary<string, FileLogo> ReadFileLogos()
        {
            Dictionary<string, Dictionary<string, string>> fileLogosJson =
                Resources.ReadEmbeddedJsonFile<Dictionary<string, Dictionary<string, string>>>(
                    typeof(TextFileRenderer).Assembly,
                    "/Resources/Data/TextFileRenderer/FileLogos/FileLogos.json");

            Dictionary<string, FileLogo> fileLogos = new();

            foreach (var fileLogoJson in fileLogosJson)
            {
                var path = SKPath.ParseSvgPathData(fileLogoJson.Value["path"]);
                var fill = SKColor.Parse(fileLogoJson.Value["fill"]);
                fileLogos[fileLogoJson.Key] = new FileLogo { Path = path, Fill = fill };
            }

            return fileLogos;
        }

        public struct FileLogo
        {
            public SKPath Path { get; set; }

            public SKColor Fill { get; set; }
        }
    }
}
