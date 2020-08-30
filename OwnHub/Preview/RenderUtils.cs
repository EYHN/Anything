using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwnHub.Preview
{
    public class RenderUtils
    {
        public static void RenderSvg(RenderContext ctx, string svgStr, SKPaint paint = null, SKPoint? point = null)
        {
            using (Svg.Skia.SKSvg svg = new Svg.Skia.SKSvg())
            {
                svg.Load(new MemoryStream(Encoding.UTF8.GetBytes(svgStr)));

                ctx.Canvas.DrawPicture(svg.Picture, point ?? new SKPoint(0,0), paint ?? new SKPaint());
            };
        }
    }
}
