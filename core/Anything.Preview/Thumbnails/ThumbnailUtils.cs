using System;
using SkiaSharp;

namespace Anything.Preview.Thumbnails;

public static class ThumbnailUtils
{
    public const int DefaultMaxWidth = 104;

    public const int DefaultMaxHeight = 88;

    public static void DrawShadowView(
        ThumbnailsRenderContext ctx,
        ISkView view,
        SKImage? decorationImage = null,
        SKColor? backgroundColor = null,
        SKSize? maxSize = null,
        SKSize? minSize = null,
        SKFilterQuality resizeQuality = SKFilterQuality.None)
    {
        // max width  = 128 - 12 - 12
        // max height = 128 - 20 - 20
        maxSize ??= new SKSize(DefaultMaxWidth, DefaultMaxHeight);

        minSize ??= new SKSize(0, 0);

        backgroundColor ??= new SKColor(0xff, 0xff, 0xff);

        var imageSize = ContainSize(new SKSize(view.Size.Width, view.Size.Height), maxSize.Value);
        var imageRect = CentralRect(imageSize);
        var borderSize = new SKSize(Math.Max(imageSize.Width, minSize.Value.Width), Math.Max(imageSize.Height, minSize.Value.Height));
        var borderRect = CentralRect(new SKSize(borderSize.Width + 1, borderSize.Height + 1));

        using (new SKAutoCanvasRestore(ctx.Canvas))
        {
            ctx.Canvas.Clear();

            using (var rectFillPaint = new SKPaint
                   {
                       Style = SKPaintStyle.Fill,
                       Color = backgroundColor.Value,
                       ImageFilter = SKImageFilter.CreateDropShadow(0, 1, 4, 4, new SKColor(136, 136, 136, 128))
                   })
            using (var rectStrokePaint = new SKPaint
                   {
                       Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(136, 136, 136, 64), BlendMode = SKBlendMode.Src
                   })
            {
                // draw border
                using SKRoundRect roundRect = new(borderRect, 5);
                ctx.Canvas.DrawRoundRect(roundRect, rectFillPaint);
                ctx.Canvas.DrawRoundRect(roundRect, rectStrokePaint);
            }

            using (new SKAutoCanvasRestore(ctx.Canvas))
            using (var imagePaint = new SKPaint { FilterQuality = resizeQuality })
            using (var decorationImagePaint = new SKPaint { FilterQuality = SKFilterQuality.Medium })
            {
                using SKRoundRect roundRect = new(imageRect, 4.5f);

                using (new SKAutoCanvasRestore(ctx.Canvas))
                {
                    ctx.Canvas.ClipRoundRect(roundRect);

                    // draw image
                    view.Draw(ctx.Canvas, imageRect, imagePaint);
                }

                if (decorationImage != null)
                {
                    var decorationRect = SKRect.Create(borderRect.Right - 24.5f, borderRect.Bottom - 22.5f, 36, 36);
                    ctx.Canvas.DrawImage(decorationImage, decorationRect, decorationImagePaint);
                }
            }
        }
    }

    public static SKSize ContainSize(SKSize contentSize, SKSize containerSize)
    {
        var ratio = contentSize.Width / contentSize.Height;

        if (ratio >= containerSize.Width / containerSize.Height)
        {
            return new SKSize(containerSize.Width, containerSize.Width / ratio);
        }
        else
        {
            return new SKSize(containerSize.Height * ratio, containerSize.Height);
        }
    }

    public static SKRect CentralRect(SKSize size)
    {
        var rectWidth = size.Width;
        var rectHeight = size.Height;
        return SKRect.Create(
            (128 - rectWidth) / 2,
            (128 - rectHeight) / 2,
            rectWidth,
            rectHeight);
    }
}
