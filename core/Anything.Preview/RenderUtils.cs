using System.IO;
using System.Text;
using SkiaSharp;
using Svg.Skia;

namespace Anything.Preview;

public static class RenderUtils
{
    public static void RenderSvg(RenderContext ctx, string svgStr, SKPaint? paint = null, SKPoint? point = null)
    {
        using SKSvg svg = new();
        using var svgStream = new MemoryStream(Encoding.UTF8.GetBytes(svgStr));
        using var svgPicture = svg.Load(svgStream);

        if (paint != null)
        {
            ctx.Canvas.DrawPicture(svgPicture, point ?? new SKPoint(0, 0), paint);
        }
        else
        {
            using var defaultPaint = new SKPaint();
            ctx.Canvas.DrawPicture(svgPicture, point ?? new SKPoint(0, 0), defaultPaint);
        }
    }
}
