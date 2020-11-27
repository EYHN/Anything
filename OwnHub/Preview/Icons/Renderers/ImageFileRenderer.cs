using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetVips;
using OwnHub.File;
using SkiaSharp;

namespace OwnHub.Preview.Icons.Renderers
{
    public class ImageFileRenderer : IDynamicIconsRenderer
    {
        public static readonly int MaxFileSize = 1024 * 1024 * 10; // 10 MB

        public static readonly string[] AllowMimeTypes =
        {
            "image/png", "image/jpeg", "image/bmp", "image/git", "image/webp"
        };

        public bool IsSupported(IFile file)
        {
            if (file is IRegularFile)
                if (AllowMimeTypes.Contains(file.MimeType?.Mime))
                    return true;
            return false;
        }

        public async Task<bool> Render(IconsRenderContext ctx, DynamicIconsRenderInfo info)
        {
            if (!IsSupported(info.File)) return false;
            if ((await info.File.Stats)?.Size > MaxFileSize) return false;

            IRegularFile file = (IRegularFile) info.File;

            var margin = 12;
            int imageMaxSize = 128 - margin * 2;
            var loadImageSize = (int) Math.Round(imageMaxSize * ctx.Density);

            Image sourceVipsImage;
            if (file is File.Local.RegularFile localFile)
                sourceVipsImage =
                    Image.Thumbnail(localFile.GetRealPath(), loadImageSize, loadImageSize, noRotate: false);
            else
                await using (Stream? stream = file.Open())
                await using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);

                    byte[] data = ms.ToArray();

                    sourceVipsImage =
                        Image.ThumbnailBuffer(data, loadImageSize, height: loadImageSize, noRotate: false);
                }

            sourceVipsImage = sourceVipsImage.Colourspace("srgb").Cast("uchar");
            if (!sourceVipsImage.HasAlpha()) sourceVipsImage = sourceVipsImage.Bandjoin(255);


            int imageWidth = sourceVipsImage.Width;
            int imageHeight = sourceVipsImage.Height;

            IntPtr sourceImageDataPtr = sourceVipsImage.WriteToMemory(out ulong byteLength);

            try
            {
                var sourceImageInfo = new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888,
                    SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());

                using (SKImage image =
                    SKImage.FromPixels(sourceImageInfo, sourceImageDataPtr, sourceImageInfo.RowBytes))
                {
                    var imageBorderSize = new SKSize(imageMaxSize, imageMaxSize);
                    float imageScale = 1;
                    if (imageWidth > imageHeight)
                    {
                        imageScale = (float) imageMaxSize / imageWidth;
                        imageBorderSize.Width = imageMaxSize;
                        imageBorderSize.Height = imageHeight * imageScale;
                    }
                    else
                    {
                        imageScale = (float) imageMaxSize / imageHeight;
                        imageBorderSize.Width = imageWidth * imageScale;
                        imageBorderSize.Height = imageMaxSize;
                    }

                    using (new SKAutoCanvasRestore(ctx.Canvas))
                    {
                        ctx.Canvas.Clear();

                        using (var rectPaint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = 1,
                            Color = new SKColor(0xe0, 0xe0, 0xe0)
                        })
                        {
                            // draw border
                            float rectWidth = imageBorderSize.Width + 1;
                            float rectHeight = imageBorderSize.Height + 1;
                            var rect = SKRect.Create((128 - rectWidth) / 2, (128 - rectHeight) / 2, rectWidth,
                                rectHeight);
                            SKRoundRect roundRect = new SKRoundRect(rect, 5);
                            ctx.Canvas.DrawRoundRect(roundRect, rectPaint);
                        }

                        {
                            float rectWidth = imageBorderSize.Width;
                            float rectHeight = imageBorderSize.Height;
                            var rect = SKRect.Create((128 - rectWidth) / 2, (128 - rectHeight) / 2, rectWidth,
                                rectHeight);
                            SKRoundRect roundRect = new SKRoundRect(rect, 4.5f);
                            ctx.Canvas.ClipRoundRect(roundRect);
                        }

                        using (var imagePaint = new SKPaint())
                        {
                            var imageRenderRect = SKRect.Create((128 - imageBorderSize.Width) / 2,
                                (128 - imageBorderSize.Height) / 2, imageBorderSize.Width, imageBorderSize.Height);
                            // draw image
                            ctx.Canvas.DrawImage(image, imageRenderRect, imagePaint);
                        }
                    }
                }
            }
            finally
            {
                NetVips.NetVips.Free(sourceImageDataPtr);
            }

            return true;
        }
    }
}