using System;
using SkiaSharp;

namespace OwnHub.Preview
{
    public class RenderContext : IDisposable
    {
        public SKCanvas Canvas;
        private bool disposed;
        public int Height = 1024;
        public int MaxHeight = 1024;
        public int MaxWidth = 1024;

        public SKSurface Surface;
        public int Width = 1024;

        public RenderContext(int maxWidth, int maxHeight)
        {
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            var info = new SKImageInfo(maxWidth, maxHeight);
            Surface = SKSurface.Create(info);
            Canvas = Surface.Canvas;
            Resize(Width, Height, false);
        }

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
            SKPixmap? pm = Surface.PeekPixels();
            return pm.ExtractSubset(new SKRectI(0, 0, Width, Height));
        }

        public SKData SnapshotPng()
        {
            SKPixmap? pm = PeekPixels();
            SKData data = pm.Encode(SKEncodedImageFormat.Png, 100);
            pm.Dispose();
            return data;
        }

        public SKData SnapshotWebp(int quality = 100)
        {
            SKPixmap? pm = PeekPixels();
            SKData data = pm.Encode(SKEncodedImageFormat.Webp, quality);
            pm.Dispose();
            return data;
        }

        public virtual void Resize(int width, int height, bool zoomContent = true)
        {
            if (width > MaxWidth || height > MaxHeight || width <= 0 || height <= 0)
                throw new Exception("Width and height out of range.");
            if (width == Width || height == Height) return;
            if (zoomContent)
            {
                SKImage? im = Snapshot();
                var paint = new SKPaint();
                paint.BlendMode = SKBlendMode.Src;
                paint.FilterQuality = SKFilterQuality.High;
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
            if (!disposed)
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
                disposed = true;
            }
        }

        ~RenderContext()
        {
            Dispose(false);
        }
    }
}