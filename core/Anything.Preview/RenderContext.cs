using System;
using SkiaSharp;

namespace Anything.Preview
{
    public class RenderContext : IDisposable
    {
        private bool _disposed;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
            if (width > MaxWidth || height > MaxHeight || width <= 0 || height <= 0)
            {
                throw new Exception("Width and height out of range.");
            }

            if (width == Width || height == Height)
            {
                return;
            }

            if (zoomContent)
            {
                var im = Snapshot();
                var paint = new SKPaint { BlendMode = SKBlendMode.Src, FilterQuality = SKFilterQuality.High };
                Canvas.ResetMatrix();
                Canvas.DrawImage(im, new SKRect(0, 0, width, height), paint);
                im.Dispose();
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

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    Canvas.Dispose();
                    Surface.Dispose();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="RenderContext" /> class.
        /// </summary>
        ~RenderContext()
        {
            Dispose(false);
        }
    }
}
