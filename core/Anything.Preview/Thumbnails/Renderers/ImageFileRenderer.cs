using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly IFileService _fileService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageFileRenderer" /> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public ImageFileRenderer(IFileService fileService)
        {
            _fileService = fileService;
        }

        protected override long MaxFileSize => 1024 * 1024 * 10; // 10 MB

        /// <inheritdoc />
        protected override ImmutableArray<MimeType.Schema.MimeType> SupportMimeTypes
        {
            get
            {
                var supportList = new List<MimeType.Schema.MimeType>(new[]
                {
                    MimeType.Schema.MimeType.image_png, MimeType.Schema.MimeType.image_jpeg, MimeType.Schema.MimeType.image_bmp,
                    MimeType.Schema.MimeType.image_gif, MimeType.Schema.MimeType.image_webp
                });
                var suffixes = NetVips.NetVips.GetOperations();
                if (suffixes.Contains("pdfload"))
                {
                    supportList.Add(MimeType.Schema.MimeType.application_pdf);
                }

                return supportList.ToImmutableArray();
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> Render(
            ThumbnailsRenderContext ctx,
            ThumbnailsRenderFileInfo fileInfo,
            ThumbnailsRenderOption option)
        {
            var margin = 12;
            var imageMaxSize = 128 - (margin * 2);
            var loadImageSize = (int)Math.Round(imageMaxSize * ctx.Density);

            Image? sourceVipsImage = null;
            try
            {
                var data = await _fileService.ReadFile(fileInfo.Url);

                // use the following code maybe faster. https://github.com/kleisauke/net-vips/issues/128
                // > sourceVipsImage = Image.Thumbnail(localPath, loadImageSize, loadImageSize, noRotate: false);
                sourceVipsImage =
                    Image.ThumbnailBuffer(data, loadImageSize, height: loadImageSize, noRotate: false);

                sourceVipsImage = sourceVipsImage.Colourspace(Enums.Interpretation.Srgb).Cast(Enums.BandFormat.Uchar);
                if (!sourceVipsImage.HasAlpha())
                {
                    sourceVipsImage = sourceVipsImage.Bandjoin(255);
                }

                var imageWidth = sourceVipsImage.Width;
                var imageHeight = sourceVipsImage.Height;

                var sourceImageDataPtr = sourceVipsImage.WriteToMemory(out _);
                sourceVipsImage.Close();

                try
                {
                    using var colorspace = SKColorSpace.CreateSrgb();
                    var sourceImageInfo = new SKImageInfo(
                        imageWidth,
                        imageHeight,
                        SKColorType.Rgba8888,
                        SKAlphaType.Unpremul,
                        colorspace);

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
                            using SKRoundRect roundRect = new(rect, 5);
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
                            using SKRoundRect roundRect = new(rect, 4.5f);
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
            }
            finally
            {
                sourceVipsImage?.Close();
            }

            return true;
        }
    }
}
