using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using StagingBox.File;
using StagingBox.Utils;
using Topten.RichTextKit;

namespace StagingBox.Preview.Icons.Renderers
{
    public class TextFileRenderer : IDynamicIconsRenderer
    {
        public static string[] SupportMimeType =
            Resources.ReadEmbeddedJsonFile<string[]>(
                FunctionUtils.GetApplicationAssembly(),
                "/Resources/Data/TextFileRenderer/SupportMimeType.json"
            );

        public static Dictionary<string, FileLogo> FileLogos = ReadFileLogos();

        private static SKBitmap? cachedBackground;

        private static readonly SKPath ClipPath = SKPath.ParseSvgPathData(
            "M72.9973 12.5L30.0318 12.5C28.0874 12.5 26.5 14.0988 26.5 16.0849V111.875C26.5 113.861 28.0874 115.46 30.0318 115.46H97.9682C99.9126 115.46 101.5 113.861 101.5 111.875V41.0027C101.5 39.932 101.031 38.9715 100.286 38.3139C99.654 37.7558 98.8246 37.4178 97.9151 37.4178H81.1671C78.6349 37.4178 76.5822 35.3651 76.5822 32.8329V16.0849C76.5822 15.0515 76.2433 13.9624 75.6429 13.35C75.0418 12.7369 74.0906 12.5 72.9973 12.5Z");

        public bool IsSupported(IFile file)
        {
            if (file is IRegularFile)
                if (SupportMimeType.Contains(file.MimeType?.Mime))
                    return true;
            return false;
        }

        public async Task<bool> Render(IconsRenderContext ctx, DynamicIconsRenderInfo info)
        {
            if (!IsSupported(info.File)) return false;

            IRegularFile file = (IRegularFile)info.File;
            using (Stream? stream = file.Open())
            {
                byte[] data = new byte[1024 * 8];
                await stream.ReadAsync(data);
                string text = Encoding.UTF8.GetString(data);

                if (cachedBackground == null) cachedBackground = SKBitmap.Decode(ReadBackgroundStream());

                int targetWidth = ctx.Width, targetHeight = ctx.Height;
                ctx.Resize(IconsConstants.MaxSize, IconsConstants.MaxSize, false);

                using (new SKAutoCanvasRestore(ctx.Canvas))
                {
                    using (var paint = new SKPaint
                    {
                        BlendMode = SKBlendMode.Src
                    })
                    {
                        ctx.Canvas.DrawBitmap(cachedBackground, SKRect.Create(0, 0, 128, 128), paint);
                    }

                    ctx.Canvas.ClipPath(ClipPath);

                    float padding = 8;
                    var tb = new Font.TextBlock();
                    float maxWidth = 76f - 2 * padding;
                    float maxHeight = 103.96f - 2 * padding;

                    tb.MaxWidth = maxWidth;
                    tb.MaxHeight = maxHeight;

                    var styleNormal = new Style
                    {
                        TextColor = new SKColor(0, 0, 0),
                        FontSize = 1.25f
                    };

                    tb.AddText(text, styleNormal);

                    var paintOptions = new TextPaintOptions
                    {
                        IsAntialias = false
                    };

                    tb.Paint(ctx.Canvas, new SKPoint((128f - maxWidth) / 2, (128f - maxHeight) / 2), paintOptions);

                    FileLogo logo;
                    if (FileLogos.TryGetValue(PathLib.Extname(file.Name), out logo) ||
                        FileLogos.TryGetValue(file.MimeType?.Mime ?? "", out logo))
                        using (var fillPaint = new SKPaint
                        {
                            Color = logo.Fill
                        })
                        using (var strokePaint = new SKPaint
                        {
                            Color = new SKColor(255, 255, 255),
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = 2
                        })
                        {
                            ctx.Canvas.DrawPath(logo.Path, strokePaint);
                            ctx.Canvas.DrawPath(logo.Path, fillPaint);
                        }
                }

                ctx.Resize(targetWidth, targetHeight);
            }

            return true;
        }

        private static Stream ReadBackgroundStream()
        {
            return Resources.ReadEmbeddedFile(FunctionUtils.GetApplicationAssembly(), "/Resources/Data/TextFileRenderer/File.png");
        }

        public static Dictionary<string, FileLogo> ReadFileLogos()
        {
            Dictionary<string, Dictionary<string, string>> fileLogosJson =
                Resources.ReadEmbeddedJsonFile<Dictionary<string, Dictionary<string, string>>>(
                    FunctionUtils.GetApplicationAssembly(),
                    "/Resources/Data/TextFileRenderer/FileLogos/FileLogos.json"
                );

            Dictionary<string, FileLogo> fileLogos = new Dictionary<string, FileLogo>();

            foreach (KeyValuePair<string, Dictionary<string, string>> fileLogoJson in fileLogosJson)
            {
                SKPath path = SKPath.ParseSvgPathData(fileLogoJson.Value["path"]);
                SKColor fill = SKColor.Parse(fileLogoJson.Value["fill"]);
                fileLogos[fileLogoJson.Key] = new FileLogo
                {
                    Path = path,
                    Fill = fill
                };
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
