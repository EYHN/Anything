using SkiaSharp;

namespace Anything.Preview.Icons;

public class IconsRenderContext : RenderContext
{
    public IconsRenderContext()
        : base(IconsConstants.MaxSize, IconsConstants.MaxSize)
    {
    }

    public float Density => (Width + Height) / 2.0f / IconsConstants.RenderSize;

    public override void Resize(int width, int height, bool zoomContent = true)
    {
        Canvas.ResetMatrix();
        base.Resize(width, height, zoomContent);
        Canvas.SetMatrix(
            SKMatrix.CreateScale(
                width / (float)IconsConstants.RenderSize,
                height / (float)IconsConstants.RenderSize));
    }
}
