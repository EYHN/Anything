using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using NetVips;
using SkiaSharp;

namespace Anything.Preview.Thumbnails.Renderers
{
    /// <summary>
    ///     Thumbnail renderer for image file.
    /// </summary>
    public class ImageFileRenderer : BaseThumbnailsRenderer
    {
        private static readonly int _maxFileSize = 1024 * 1024 * 10; // 10 MB
        private readonly IFileSystemService _fileSystem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageFileRenderer" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system service.</param>
        public ImageFileRenderer(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <inheritdoc />
        protected override string[] SupportMimeTypes { get; } = { "image/png", "image/jpeg", "image/bmp", "image/git", "image/webp" };

        /// <inheritdoc />
        public override async Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderOption option)
        {
            var stats = await _fileSystem.Stat(option.Url);
            if (stats.Size > _maxFileSize)
            {
                return false;
            }

            var margin = 12;
            var imageMaxSize = 128 - (margin * 2);
            var loadImageSize = (int)Math.Round(imageMaxSize * ctx.Density);

            Image sourceVipsImage;
            var localPath = _fileSystem.ToLocalPath(option.Url);
            if (localPath != null)
            {
                sourceVipsImage =
                    Image.Thumbnail(localPath, loadImageSize, loadImageSize, noRotate: false);
            }
            else
            {
                var data = await _fileSystem.ReadFile(option.Url);

                sourceVipsImage =
                    Image.ThumbnailBuffer(data, loadImageSize, height: loadImageSize, noRotate: false);
            }

            sourceVipsImage = sourceVipsImage.Colourspace(Enums.Interpretation.Srgb).Cast(Enums.BandFormat.Uchar);
            if (!sourceVipsImage.HasAlpha())
            {
                sourceVipsImage = sourceVipsImage.Bandjoin(255);
            }

            var imageWidth = sourceVipsImage.Width;
            var imageHeight = sourceVipsImage.Height;

            var sourceImageDataPtr = sourceVipsImage.WriteToMemory(out _);

            try
            {
                var sourceImageInfo = new SKImageInfo(
                    imageWidth,
                    imageHeight,
                    SKColorType.Rgba8888,
                    SKAlphaType.Unpremul,
                    SKColorSpace.CreateSrgb());

                using var image =
                    SKImage.FromPixels(sourceImageInfo, sourceImageDataPtr, sourceImageInfo.RowBytes);
                var imageBorderSize = new SKSize(imageMaxSize, imageMaxSize);
                float imageScale;
                if (imageWidth > imageHeight)
                {
                    imageScale = (float)imageMaxSize / imageWidth;
                    imageBorderSize.Width = imageMaxSize;
                    imageBorderSize.Height = imageHeight * imageScale;
                }
                else
                {
                    imageScale = (float)imageMaxSize / imageHeight;
                    imageBorderSize.Width = imageWidth * imageScale;
                    imageBorderSize.Height = imageMaxSize;
                }

                using (new SKAutoCanvasRestore(ctx.Canvas))
                {
                    ctx.Canvas.Clear();

                    using (var rectPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(0xe0, 0xe0, 0xe0)
                    })
                    {
                        // draw border
                        var rectWidth = imageBorderSize.Width + 1;
                        var rectHeight = imageBorderSize.Height + 1;
                        var rect = SKRect.Create(
                            (128 - rectWidth) / 2,
                            (128 - rectHeight) / 2,
                            rectWidth,
                            rectHeight);
                        SKRoundRect roundRect = new(rect, 5);
                        ctx.Canvas.DrawRoundRect(roundRect, rectPaint);
                    }

                    {
                        var rectWidth = imageBorderSize.Width;
                        var rectHeight = imageBorderSize.Height;
                        var rect = SKRect.Create(
                            (128 - rectWidth) / 2,
                            (128 - rectHeight) / 2,
                            rectWidth,
                            rectHeight);
                        SKRoundRect roundRect = new(rect, 4.5f);
                        ctx.Canvas.ClipRoundRect(roundRect);
                    }

                    using (var imagePaint = new SKPaint())
                    {
                        var imageRenderRect = SKRect.Create(
                            (128 - imageBorderSize.Width) / 2,
                            (128 - imageBorderSize.Height) / 2,
                            imageBorderSize.Width,
                            imageBorderSize.Height);

                        // draw image
                        ctx.Canvas.DrawImage(image, imageRenderRect, imagePaint);
                    }
                }
            }
            finally
            {
                NetVips.NetVips.Free(sourceImageDataPtr);
            }

            return true;
        }
    }
}
