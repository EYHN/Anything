using OwnHub.File;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Icons.Renderers
{
    public class ImageFileRenderer : IDynamicIconsRenderer
    {
        public static readonly int MaxFileSize = 1024 * 1024 * 10; // 10 MB

        public static readonly string[] AllowMimeTypes = new string[]
        {
            "image/png", "image/jpeg", "image/bmp", "image/git", "image/webp"
        };

        public bool IsSupported(IFile file)
        {
            if (file is IRegularFile)
            {
                if (AllowMimeTypes.Contains(file.MimeType?.Mime))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> Render(IconsRenderContext ctx, DynamicIconsRenderInfo info)
        {
            if (!IsSupported(info.file)) return false;
            if ((await info.file.Stats)?.Size > MaxFileSize) return false;

            IRegularFile file = (IRegularFile)info.file;

            int Margin = 12;
            int ImageMaxSize = 128 - Margin * 2;

            NetVips.Image SourceVipsImage;
            if (file is File.Local.RegularFile)
            {

                SourceVipsImage = NetVips.Image.Thumbnail((file as File.Local.RegularFile).GetRealPath(), width: ImageMaxSize * 8, height: ImageMaxSize * 8, noRotate: false);

            }
            else
            {
                using (var stream = file.Open())
                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);

                    byte[] Data = ms.ToArray();

                    SourceVipsImage = NetVips.Image.ThumbnailBuffer(Data, width: ImageMaxSize * 8, height: ImageMaxSize * 8, noRotate: false);
                }
            }

            SourceVipsImage = SourceVipsImage.Colourspace("srgb").Cast("uchar");
            if (!SourceVipsImage.HasAlpha())
            {
                SourceVipsImage = SourceVipsImage.Bandjoin(255);
            }


            int ImageWidth = SourceVipsImage.Width;
            int ImageHeight = SourceVipsImage.Height;

            IntPtr SourceImageDataPtr = SourceVipsImage.WriteToMemory(out var ByteLength);

            try
            {
                SKImageInfo SourceImageInfo = new SKImageInfo(ImageWidth, ImageHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());

                using (SKImage Image = SKImage.FromPixels(SourceImageInfo, SourceImageDataPtr, SourceImageInfo.RowBytes))
                {

                    SKSize ImageBorderSize = new SKSize(ImageMaxSize, ImageMaxSize);
                    float imageScale = 1;
                    if (ImageWidth > ImageHeight)
                    {
                        imageScale = (float)ImageMaxSize / ImageWidth;
                        ImageBorderSize.Width = ImageMaxSize;
                        ImageBorderSize.Height = ImageHeight * imageScale;
                    }
                    else
                    {
                        imageScale = (float)ImageMaxSize / ImageHeight;
                        ImageBorderSize.Width = ImageWidth * imageScale;
                        ImageBorderSize.Height = ImageMaxSize;
                    }

                    using (new SKAutoCanvasRestore(ctx.Canvas))
                    {
                        ctx.Canvas.Clear();

                        using (var RectPaint = new SKPaint()
                        {
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = 1,
                            Color = new SKColor(0xe0, 0xe0, 0xe0)
                        })
                        {
                            // draw border
                            var RectWidth = ImageBorderSize.Width + 1;
                            var RectHeight = ImageBorderSize.Height + 1;
                            SKRect Rect = SKRect.Create((128 - RectWidth) / 2, (128 - RectHeight) / 2, RectWidth, RectHeight);
                            SKRoundRect RoundRect = new SKRoundRect(Rect, 5);
                            ctx.Canvas.DrawRoundRect(RoundRect, RectPaint);
                        }

                        {
                            var RectWidth = ImageBorderSize.Width;
                            var RectHeight = ImageBorderSize.Height;
                            SKRect Rect = SKRect.Create((128 - RectWidth) / 2, (128 - RectHeight) / 2, RectWidth, RectHeight);
                            SKRoundRect RoundRect = new SKRoundRect(Rect, 4.5f);
                            ctx.Canvas.ClipRoundRect(RoundRect);
                        }

                        using (var ImagePaint = new SKPaint()
                        {
                        })
                        {
                            var ImageRenderRect = SKRect.Create((128 - ImageBorderSize.Width) / 2, (128 - ImageBorderSize.Height) / 2, ImageBorderSize.Width, ImageBorderSize.Height);
                            // draw image
                            ctx.Canvas.DrawImage(Image, ImageRenderRect, ImagePaint);
                        }
                    }
                }
            }
            finally
            {
                NetVips.NetVips.Free(SourceImageDataPtr);
            }

            return true;
        }
    }
}