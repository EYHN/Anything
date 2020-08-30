using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Icons
{
    public class IconsRenderContext: RenderContext
    {
        public IconsRenderContext(): base(IconsConstants.MaxSize, IconsConstants.MaxSize)
        {
        }

        public override void Resize(int width, int height, bool zoomContent = true)
        {
            Canvas.ResetMatrix();
            base.Resize(width, height, zoomContent);
            Canvas.SetMatrix(SKMatrix.CreateScale(width / (float)IconsConstants.RenderSize, height / (float)IconsConstants.RenderSize));
        }
    }
}
