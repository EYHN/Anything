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
        public static string[] AllowMimeTypes = new string[]
        {
            "text/plain",
            "application/json",
            "application/javascript",
            "text/x-csharp",
            "text/x-go",
            "application/x-php",
            "text/x-java",
            "text/x-csrc",
            "text/x-chdr",
            "text/x-c++src",
            "text/x-c++hdr",
            "text/x-python",
            "text/x-pascal",
            "text/css"
        };

        public static Dictionary<string, FileLogo> FileLogos = ReadFileLogos();

        public bool FileFilter(IFile file)
        {
            if (file is IRegularFile)
            {
                if (AllowMimeTypes.Contains(file.MimeType.Mime))
                {
                    return true;
                }
            }
            return false;
        }

        private static SKBitmap cachedBackground;

        private static SKPath ClipPath = SKPath.ParseSvgPathData("M103.083 116H24V12H56.9514C65.0923 12 67.0306 12.7761 70.1319 15.1045C70.3857 15.295 70.7458 15.5945 71.1954 15.9864C74.9549 20.6524 74.9227 23.619 74.8072 34.2509C74.8012 34.7967 74.7951 35.3626 74.7892 35.95C76.6479 35.9556 78.2416 35.8394 79.717 35.7318C84.6311 35.3736 88.2313 35.1111 95.9285 39.765C96.1379 39.9734 96.3267 40.1618 96.493 40.3284C100.757 44.597 103.083 47.3134 103.083 64V116Z");

        private static Stream ReadBackgroundStream()
        {
            return new FileStream(Utils.Utils.GetApplicationRoot() + "/Resources/Data/TextFileRenderer/File.png", FileMode.Open);
        }

        public class FileLogo
        {
            public SKPath Path { get; set; }
            public SKColor Fill { get; set; }
        }

        public static Dictionary<string, FileLogo> ReadFileLogos()
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            Dictionary<string, Dictionary<string, string>> FileLogosJson = JsonSerializer.Deserialize< Dictionary<string, Dictionary<string, string>>>(
                System.IO.File.ReadAllText(Path.Join(Utils.Utils.GetApplicationRoot(), "Resources/Data/TextFileRenderer/FileLogos/data.json")),
                options
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
            if (!FileFilter(info.file)) return false;

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
                using (var paint = new SKPaint()
                {
                    BlendMode = SKBlendMode.Src
                })
                {
                    ctx.Canvas.DrawBitmap(cachedBackground, SKRect.Create(0, 0, 128, 128), paint);
                }
                
                ctx.Canvas.ClipPath(ClipPath);

                float padding = 4;
                var tb = new TextBlock();

                tb.MaxWidth = 79.08f - 2f * padding;
                tb.MaxHeight = 104 - 2 * padding;

                var styleNormal = new Style()
                {
                    TextColor = new SKColor(0, 0, 0),
                    FontFamily = "Arial",
                    FontSize = 1.25f
                };

                tb.AddText(text, styleNormal);

                tb.Paint(ctx.Canvas, new SKPoint(24 + padding, 12 + padding));

                FileLogo Logo;
                if (FileLogos.TryGetValue(PathUtils.Extname(file.Name), out Logo) ||
                    FileLogos.TryGetValue(file.MimeType.Mime, out Logo))
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

            return true;
        }
    }
}
