using System;
using Anything.Utils;
using SkiaSharp;

namespace Anything.Preview
{
    public class RenderContext : Disposable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RenderContext" /> class.
        /// </summary>
        /// <param name="maxWidth">The max width of the render context.</param>
        /// <param name="maxHeight">The max height of the render context.</param>
        public RenderContext(int maxWidth, int maxHeight)
        {
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            var info = new SKImageInfo(maxWidth, maxHeight);
            Surface = SKSurface.Create(info);
            Canvas = Surface.Canvas;
            Resize(Width, Height, false);
        }

        public SKCanvas Canvas { get; }

        public int MaxHeight { get; }

        public int MaxWidth { get; }

        public SKSurface Surface { get; }

        public int Width { get; private set; } = 1024;

        public int Height { get; private set; } = 1024;

        public virtual SKImage Snapshot()
        {
            return Surface.Snapshot(new SKRectI(0, 0, Width, Height));
        }

        public virtual SKPixmap PeekPixels()
        {
            var pm = Surface.PeekPixels();
            return pm.ExtractSubset(new SKRectI(0, 0, Width, Height));
        }

        public SKData SnapshotPng()
        {
            var pm = PeekPixels();
            var data = pm.Encode(SKEncodedImageFormat.Png, 100);
            pm.Dispose();
            return data;
        }

        public SKData SnapshotWebp(int quality = 100)
        {
            var pm = PeekPixels();
            var data = pm.Encode(SKEncodedImageFormat.Webp, quality);
            pm.Dispose();
            return data;
        }

        public virtual void Resize(int width, int height, bool zoomContent = true)
        {
            if (width > MaxWidth || width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width out of range.");
            }

            if (height > MaxHeight || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height out of range.");
            }

            if (width == Width || height == Height)
            {
                return;
            }

            if (zoomContent)
            {
                using var im = Snapshot();
                using var paint = new SKPaint { BlendMode = SKBlendMode.Src, FilterQuality = SKFilterQuality.High };
                Canvas.ResetMatrix();
                Canvas.DrawImage(im, new SKRect(0, 0, width, height), paint);
            }

            Width = width;
            Height = height;
        }

        public virtual void Save()
        {
            Canvas.Save();
        }

        public virtual void Restore()
        {
            Canvas.Restore();
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            Canvas.Dispose();
            Surface.Dispose();
        }
    }
}
