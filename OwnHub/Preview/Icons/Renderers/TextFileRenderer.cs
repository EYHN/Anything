using OwnHub.File;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Topten.RichTextKit;

namespace OwnHub.Preview.Icons.Renderers
{
    public class TextFileRenderer : IDynamicIconsRenderer
    {
        public static string[] SupportMimeType =
            Utils.Utils.DeserializeEmbeddedJsonFile<string[]>(
                "/Resources/Data/TextFileRenderer/SupportMimeType.json"
            );

        public static Dictionary<string, FileLogo> FileLogos = ReadFileLogos();

        public bool IsSupported(IFile file)
        {
            if (file is IRegularFile)
            {
                if (SupportMimeType.Contains(file.MimeType?.Mime))
                {
                    return true;
                }
            }
            return false;
        }

        private static SKBitmap cachedBackground;

        private static SKPath ClipPath = SKPath.ParseSvgPathData("M72.9973 12.5L30.0318 12.5C28.0874 12.5 26.5 14.0988 26.5 16.0849V111.875C26.5 113.861 28.0874 115.46 30.0318 115.46H97.9682C99.9126 115.46 101.5 113.861 101.5 111.875V41.0027C101.5 39.932 101.031 38.9715 100.286 38.3139C99.654 37.7558 98.8246 37.4178 97.9151 37.4178H81.1671C78.6349 37.4178 76.5822 35.3651 76.5822 32.8329V16.0849C76.5822 15.0515 76.2433 13.9624 75.6429 13.35C75.0418 12.7369 74.0906 12.5 72.9973 12.5Z");

        private static Stream ReadBackgroundStream()
        {
            return Utils.Utils.ReadEmbeddedFile("/Resources/Data/TextFileRenderer/File.png");
        }

        public struct FileLogo
        {
            public SKPath Path { get; set; }
            public SKColor Fill { get; set; }
        }

        public static Dictionary<string, FileLogo> ReadFileLogos()
        {
            Dictionary<string, Dictionary<string, string>> FileLogosJson = 
                Utils.Utils.DeserializeEmbeddedJsonFile<Dictionary<string, Dictionary<string, string>>>(
                    "/Resources/Data/TextFileRenderer/FileLogos/FileLogos.json"
                );

            Dictionary<string, FileLogo> FileLogos = new Dictionary<string, FileLogo>();

            foreach (var FileLogoJson in FileLogosJson)
            {
                SKPath Path = SKPath.ParseSvgPathData(FileLogoJson.Value["path"]);
                SKColor Fill = SKColor.Parse(FileLogoJson.Value["fill"]);
                FileLogos[FileLogoJson.Key] = new FileLogo() {
                    Path = Path,
                    Fill = Fill
                };
            }

            return FileLogos;
        }

        public async Task<bool> Render(IconsRenderContext ctx, DynamicIconsRenderInfo info)
        {
            if (!IsSupported(info.file)) return false;

            IRegularFile file = (IRegularFile)info.file;
            using (var stream = file.Open())
            {
                byte[] data = new byte[1024*8];
                await stream.ReadAsync(data);
                string text = Encoding.UTF8.GetString(data);

                if (cachedBackground == null)
                {
                    cachedBackground = SKBitmap.Decode(ReadBackgroundStream());
                }

                int TargetWidth = ctx.Width, TargetHeight = ctx.Height;
                ctx.Resize(IconsConstants.MaxSize, IconsConstants.MaxSize, false);

                using (new SKAutoCanvasRestore(ctx.Canvas))
                {
                    using (var paint = new SKPaint()
                    {
                        BlendMode = SKBlendMode.Src
                    })
                    {
                        ctx.Canvas.DrawBitmap(cachedBackground, SKRect.Create(0, 0, 128, 128), paint);
                    }

                    ctx.Canvas.ClipPath(ClipPath);

                    float padding = 8;
                    var tb = new Font.TextBlock();
                    float MaxWidth = 76f - 2 * padding;
                    float MaxHeight = 103.96f - 2 * padding;

                    tb.MaxWidth = MaxWidth;
                    tb.MaxHeight = MaxHeight;

                    var styleNormal = new Style()
                    {
                        TextColor = new SKColor(0, 0, 0),
                        FontSize = 1.25f
                    };

                    tb.AddText(text, styleNormal);

                    var paintOptions = new TextPaintOptions()
                    {
                        IsAntialias = false
                    };

                    tb.Paint(ctx.Canvas, new SKPoint((128f - MaxWidth) / 2, (128f - MaxHeight) / 2), paintOptions);

                    FileLogo Logo;
                    if (FileLogos.TryGetValue(PathUtils.Extname(file.Name), out Logo) ||
                        FileLogos.TryGetValue(file.MimeType?.Mime, out Logo))
                    {
                        using (var FillPaint = new SKPaint()
                        {
                            Color = Logo.Fill
                        })
                        using (var StrokePaint = new SKPaint()
                        {
                            Color = new SKColor(255, 255, 255),
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = 2
                        })
                        {
                            ctx.Canvas.DrawPath(Logo.Path, StrokePaint);
                            ctx.Canvas.DrawPath(Logo.Path, FillPaint);
                        }
                    }
                }

                ctx.Resize(TargetWidth, TargetHeight, true);
            }

            return true;
        }
    }
}
