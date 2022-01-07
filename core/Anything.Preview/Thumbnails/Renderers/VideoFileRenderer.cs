using System;
using System.IO;
using System.Threading.Tasks;
using Anything.FFmpeg;
using Anything.FileSystem;
using Anything.Utils;
using SkiaSharp;

namespace Anything.Preview.Thumbnails.Renderers;

public class VideoFileRenderer : IThumbnailsRenderer
{
    private static SKImage? _cachedDecorationImage;
    private readonly IFileService _fileService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="VideoFileRenderer" /> class.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    public VideoFileRenderer(IFileService fileService)
    {
        _fileService = fileService;
        FFmpegHelper.SetupFFmpegLibraryLoader();
    }

    public virtual bool IsSupported(ThumbnailsRenderFileInfo fileInfo)
    {
        if (fileInfo.Type.HasFlag(FileType.File) && fileInfo.MimeType != null &&
            fileInfo.MimeType.Mime.StartsWith("video/", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    public async Task<bool> Render(
        ThumbnailsRenderContext ctx,
        ThumbnailsRenderFileInfo fileInfo,
        ThumbnailsRenderOption option)
    {
        await _fileService.ReadFileStream(
            fileInfo.FileHandle,
            videoStream => ValueTask.FromResult(DrawVideo(videoStream, ctx)));

        return true;
    }

    private static unsafe bool DrawVideo(Stream videoStream, ThumbnailsRenderContext ctx)
    {
        using var formatContext = new FormatContext(videoStream);
        var stream = formatContext.FindBestVideoStream();
        if (stream == null)
        {
            return false;
        }

        using var videoStreamDecoder = stream.CreateStreamDecoder();

        try
        {
            if (videoStreamDecoder.Duration <= 0)
            {
                videoStreamDecoder.SeekFrame(10 * 1000000);
            }

            if (videoStreamDecoder.Duration > 3)
            {
                videoStreamDecoder.SeekFrame(videoStreamDecoder.Duration / 3);
            }
        }
        catch (FFmpegException err)
        {
            Console.WriteLine("Seek failed: " + err);
        }

        var destinationSize = ThumbnailUtils.ContainSize(
            new SKSize(videoStreamDecoder.FrameWidth, videoStreamDecoder.FrameHeight),
            new SKSize(ThumbnailUtils.DefaultMaxWidth * ctx.Density, ThumbnailUtils.DefaultMaxHeight * ctx.Density)).ToSizeI();

        var sourcePixelFormat = videoStreamDecoder.PixelFormat;

        if (!videoStreamDecoder.MoveNext())
        {
            throw new InvalidDataException("Can't decode the video.");
        }

        using var vfc =
            new VideoFrameConverter(
                videoStreamDecoder.FrameWidth,
                videoStreamDecoder.FrameHeight,
                sourcePixelFormat,
                destinationSize.Width,
                destinationSize.Height);

        var convertedFrame = vfc.Convert(videoStreamDecoder.Current.Value);

        using var colorspace = SKColorSpace.CreateSrgb();

        var sourceImageInfo = new SKImageInfo(
            convertedFrame.width,
            convertedFrame.height,
            SKColorType.Rgba8888,
            SKAlphaType.Unpremul,
            colorspace);

        using var image =
            SKImage.FromPixels(sourceImageInfo, (IntPtr)convertedFrame.data[0], sourceImageInfo.RowBytes);

        _cachedDecorationImage ??= SKImage.FromEncodedData(ReadDecorationImage());
        ThumbnailUtils.DrawShadowView(
            ctx,
            new SkImageView(image),
            _cachedDecorationImage,
            new SKColor(0, 0, 0),
            minSize: new SKSize(24, 24));
        return true;
    }

    private static byte[] ReadDecorationImage()
    {
        return Resources.ReadEmbeddedFile(typeof(VideoFileRenderer).Assembly, "/Shared/design/generated/thumbnails/video/decoration.png");
    }
}
