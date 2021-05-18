using SkiaSharp;

namespace StagingBox.Preview.Thumbnails
{
    public class ThumbnailsRenderContext : RenderContext
    {
        public ThumbnailsRenderContext()
            : base(ThumbnailsConstants.MaxSize, ThumbnailsConstants.MaxSize)
        {
        }

        public float Density => (Width + Height) / 2.0f / ThumbnailsConstants.RenderSize;

        public override void Resize(int width, int height, bool zoomContent = true)
        {
            Canvas.ResetMatrix();
            base.Resize(width, height, zoomContent);
            Canvas.SetMatrix(
                SKMatrix.CreateScale(
                    width / (float)ThumbnailsConstants.RenderSize,
                    height / (float)ThumbnailsConstants.RenderSize));
        }
    }
}
