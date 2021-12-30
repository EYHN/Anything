using System;
using System.IO;
using System.Threading.Tasks;
using Anything.FFmpeg;
using Anything.FileSystem;
using Anything.Utils;
using NetVips;
using SkiaSharp;

namespace Anything.Preview.Thumbnails.Renderers;

public class AudioFileRenderer : IThumbnailsRenderer
{
    private static SKImage? _cachedDecorationImage;
    private readonly IFileService _fileService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AudioFileRenderer" /> class.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    public AudioFileRenderer(IFileService fileService)
    {
        _fileService = fileService;
        FFmpegHelper.SetupFFmpegLibraryLoader();
    }

    public virtual bool IsSupported(ThumbnailsRenderFileInfo fileInfo)
    {
        if (fileInfo.Type.HasFlag(FileType.File) && fileInfo.MimeType != null &&
            fileInfo.MimeType.Mime.StartsWith("audio/", StringComparison.Ordinal))
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
        await _fileService.ReadFileStream(fileInfo.FileHandle, videoStream =>
        {
            using var formatContext = new FormatContext(videoStream);

            var attachedPicStream = formatContext.FindAttachedPicStream();

            if (attachedPicStream != null)
            {
                DrawAttachedPicture(ctx, attachedPicStream);
            }
            else
            {
                var audioStream = formatContext.FindBestAudioStream();

                if (audioStream == null)
                {
                    throw new InvalidDataException("No audio stream found");
                }

                DrawWaves(ctx, audioStream);
            }

            return ValueTask.FromResult(true);
        });

        return true;
    }

    private static unsafe void DrawWaves(ThumbnailsRenderContext ctx, MediaStream audioStream)
    {
        var height = 88;
        var width = 88;
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(SKRect.Create(width, height));
        var columnCount = 88 * 2;
        var columnWidth = (float)width / columnCount;
        using var wavePaint = new SKPaint { Color = new SKColor(218, 218, 218, 255), StrokeWidth = columnWidth };
        var totalSample = audioStream.SampleRate * audioStream.Duration;
        var columnMaxSample = (int)(totalSample / columnCount);
        var columns = new short[columnCount];

        using var decoder = audioStream.CreateStreamDecoder();
        using var filter = new AudioFormatFilter("sample_fmts=s16:channel_layouts=mono", audioStream, decoder);
        filter.Build();

        long sum = 0;
        var n = 0;
        var c = 0;
        while (filter.MoveNext())
        {
            var frame = filter.Current.Value;
            var p = (short*)frame->data[0];

            for (var i = 0; i < frame->nb_samples; i++)
            {
                sum += SampleAbs(p[i]);

                n++;
                if (n == columnMaxSample && c < columnCount)
                {
                    columns[c] = (short)(sum / n);

                    n = 0;
                    sum = 0;
                    c++;
                }
            }
        }

        for (var i = 0; i < columnCount; i++)
        {
            var h = Math.Max((float)columns[i] * height / short.MaxValue, 0.5f);
            var x = i * columnWidth;
            canvas.DrawLine(x, (height - h) / 2.0f, x, (height + h) / 2.0f, wavePaint);
        }

        using var picture = recorder.EndRecording();
        _cachedDecorationImage ??= SKImage.FromEncodedData(ReadDecorationImage());
        ThumbnailUtils.DrawShadowView(
            ctx,
            new SkPictureView(
                picture,
                new SKSize(88, 88)),
            _cachedDecorationImage);
    }

    private static short SampleAbs(short value)
    {
        if (value >= 0)
        {
            return value;
        }

        if (value == short.MinValue)
        {
            return short.MaxValue;
        }

        return (short)(-value);
    }

    private static void DrawAttachedPicture(ThumbnailsRenderContext ctx, MediaStream attachedPicStream)
    {
        using var attachedPicture = attachedPicStream.ReadAttachedPicture();
        var attachedPictureVipsImage = Image.ThumbnailStream(
            attachedPicture,
            (int)(ThumbnailUtils.DefaultMaxWidth * ctx.Density),
            height: (int)(ThumbnailUtils.DefaultMaxHeight * ctx.Density),
            noRotate: false);

        attachedPictureVipsImage = attachedPictureVipsImage.Colourspace(Enums.Interpretation.Srgb).Cast(Enums.BandFormat.Uchar);
        if (!attachedPictureVipsImage.HasAlpha())
        {
            attachedPictureVipsImage = attachedPictureVipsImage.Bandjoin(255);
        }

        var imageWidth = attachedPictureVipsImage.Width;
        var imageHeight = attachedPictureVipsImage.Height;

        var sourceImageDataPtr = attachedPictureVipsImage.WriteToMemory(out _);
        attachedPictureVipsImage.Close();

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
            _cachedDecorationImage ??= SKImage.FromEncodedData(ReadDecorationImage());
            ThumbnailUtils.DrawShadowView(
                ctx,
                new SkImageView(image),
                _cachedDecorationImage,
                new SKColor(0, 0, 0),
                minSize: new SKSize(24, 24));
        }
        finally
        {
            NetVips.NetVips.Free(sourceImageDataPtr);
        }
    }

    private static byte[] ReadDecorationImage()
    {
        return Resources.ReadEmbeddedFile(typeof(VideoFileRenderer).Assembly, "/Shared/design/generated/thumbnails/audio/decoration.png");
    }
}
