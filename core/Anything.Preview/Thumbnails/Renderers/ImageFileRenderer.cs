using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Mime.Schema;
using NetVips;
using SkiaSharp;

namespace Anything.Preview.Thumbnails.Renderers;

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
    protected override ImmutableArray<MimeType> SupportMimeTypes
    {
        get
        {
            var supportList = new List<MimeType>(new[]
            {
                MimeType.image_png, MimeType.image_jpeg, MimeType.image_bmp, MimeType.image_gif, MimeType.image_webp
            });
            var suffixes = NetVips.NetVips.GetOperations();
            if (suffixes.Contains("pdfload"))
            {
                supportList.Add(MimeType.application_pdf);
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
        // use the following code maybe faster. https://github.com/kleisauke/net-vips/issues/128
        // > sourceVipsImage = Image.Thumbnail(localPath, loadImageSize, loadImageSize, noRotate: false);
        return await _fileService.ReadFileStream(
            fileInfo.FileHandle,
            stream =>
            {
                var sourceVipsImage = Image.ThumbnailStream(
                    stream,
                    (int)(ThumbnailUtils.DefaultMaxWidth * ctx.Density),
                    height: (int)(ThumbnailUtils.DefaultMaxHeight * ctx.Density),
                    noRotate: false);

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
                    ThumbnailUtils.DrawShadowView(ctx, new SkImageView(image));
                }
                finally
                {
                    NetVips.NetVips.Free(sourceImageDataPtr);
                }

                return ValueTask.FromResult(true);
            });
    }
}
