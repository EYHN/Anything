using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview
{
    public class RenderContext : IDisposable
    {
        private bool disposed = false;
        public int MaxWidth = 1024;
        public int MaxHeight = 1024;
        public int Width = 1024;
        public int Height = 1024;

        public SKSurface Surface;
        public SKCanvas Canvas;

        public RenderContext(int maxWidth, int maxHeight)
        {
            this.MaxWidth = maxWidth;
            this.MaxHeight = maxHeight;
            SKImageInfo info = new SKImageInfo(maxWidth, maxHeight);
            Surface = SKSurface.Create(info);
            Canvas = Surface.Canvas;
            this.Resize(Width, Height, false);
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

        public SKData SnapshotPNG()
        {
            var pm = PeekPixels();
            SKData data = pm.Encode(SKEncodedImageFormat.Png, 100);
            pm.Dispose();
            return data;
        }

        public SKData SnapshotWEBP(int quality = 100)
        {
            var pm = PeekPixels();
            SKData data = pm.Encode(SKEncodedImageFormat.Webp, quality);
            pm.Dispose();
            return data;
        }

        public virtual void Resize(int width, int height, bool zoomContent = true)
        {
            if (width > MaxWidth || height > MaxHeight || width <= 0 || height <= 0)
            {
                throw new Exception("Width and height out of range.");
            }
            if (width == this.Width || height == this.Height) return;
            if (zoomContent)
            {
                var im = Snapshot();
                var paint = new SKPaint();
                paint.BlendMode = SKBlendMode.Src;
                paint.FilterQuality = SKFilterQuality.High;
                Canvas.ResetMatrix();
                Canvas.DrawImage(im, new SKRect(0, 0, width, height), paint);
                im.Dispose();
            }
            this.Width = width;
            this.Height = height;
        }

        public virtual void Save()
        {
            this.Canvas.Save();
        }

        public virtual void Restore()
        {
            this.Canvas.Restore();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
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
