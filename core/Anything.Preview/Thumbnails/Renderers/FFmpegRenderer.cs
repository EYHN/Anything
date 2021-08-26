using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Anything.FFmpeg;
using Anything.FileSystem;
using Anything.Preview.Mime.Schema;
using FFmpeg.AutoGen;
using SkiaSharp;

namespace Anything.Preview.Thumbnails.Renderers
{
    public class FFmpegRenderer : BaseThumbnailsRenderer
    {
        private const int Margin = 12;
        private const int ImageMaxSize = 128 - (Margin * 2);
        private readonly IFileService _fileService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FFmpegRenderer" /> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public FFmpegRenderer(IFileService fileService)
        {
            _fileService = fileService;
            FFmpegHelper.SetupFFmpegLibraryLoader();
        }

        protected override ImmutableArray<MimeType> SupportMimeTypes => ImmutableArray.Create(MimeType.video_mp4);

        protected override async Task<bool> Render(
            ThumbnailsRenderContext ctx,
            ThumbnailsRenderFileInfo fileInfo,
            ThumbnailsRenderOption option)
        {
            var loadImageSize = (int)Math.Round(ImageMaxSize * ctx.Density);

            await _fileService.ReadFileStream(fileInfo.Url, videoStream =>
            {
                using var videoStreamDecoder = new VideoStreamDecoder(videoStream);

                try
                {
                    if (videoStreamDecoder.Duration >= 0)
                    {
                        videoStreamDecoder.SeekFrame(videoStreamDecoder.Duration / 3);
                    }
                    else
                    {
                        videoStreamDecoder.SeekFrame(10 * 1000000);
                    }
                }
                catch (FFmpegException err)
                {
                    Console.WriteLine("Seek failed: " + err);
                }

                var sourceWidth = videoStreamDecoder.FrameWidth;
                var sourceHeight = videoStreamDecoder.FrameHeight;
                int destinationWidth, destinationHeight;
                float imageScale;
                if (sourceWidth > sourceHeight)
                {
                    imageScale = (float)loadImageSize / sourceWidth;
                    destinationWidth = loadImageSize;
                    destinationHeight = (int)Math.Floor(sourceHeight * imageScale);
                }
                else
                {
                    imageScale = (float)loadImageSize / sourceHeight;
                    destinationWidth = (int)Math.Floor(sourceWidth * imageScale);
                    destinationHeight = loadImageSize;
                }

                var sourcePixelFormat = videoStreamDecoder.PixelFormat;
                using var vfc =
                    new VideoFrameConverter(
                        sourceWidth,
                        sourceHeight,
                        sourcePixelFormat,
                        destinationWidth,
                        destinationHeight);

                if (!videoStreamDecoder.TryDecodeNextFrame(out var frame))
                {
                    throw new InvalidDataException("Can't decode the video.");
                }

                var convertedFrame = vfc.Convert(frame);

                DrawAvFrame(convertedFrame, ctx.Canvas);

                return ValueTask.CompletedTask;
            });

            return true;
        }

        private static unsafe void DrawAvFrame(AVFrame frame, SKCanvas canvas)
        {
            using var colorspace = SKColorSpace.CreateSrgb();

            var sourceImageInfo = new SKImageInfo(
                frame.width,
                frame.height,
                SKColorType.Rgba8888,
                SKAlphaType.Unpremul,
                colorspace);

            using var image =
                SKImage.FromPixels(sourceImageInfo, (IntPtr)frame.data[0], sourceImageInfo.RowBytes);

            var imageBorderSize = new SKSize(ImageMaxSize, ImageMaxSize);

            float imageScale;
            if (frame.width > frame.height)
            {
                imageScale = (float)ImageMaxSize / frame.width;
                imageBorderSize.Width = ImageMaxSize;
                imageBorderSize.Height = frame.height * imageScale;
            }
            else
            {
                imageScale = (float)ImageMaxSize / frame.height;
                imageBorderSize.Width = frame.width * imageScale;
                imageBorderSize.Height = ImageMaxSize;
            }

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Clear();

                using (var rectFillPaint = new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(0xff, 0xff, 0xff) })
                using (var rectStrokePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(136, 136, 136, 64), BlendMode = SKBlendMode.Src
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
                    canvas.DrawRoundRect(roundRect, rectFillPaint);
                    canvas.DrawRoundRect(roundRect, rectStrokePaint);
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
                    canvas.ClipRoundRect(roundRect);
                }

                using (var imagePaint = new SKPaint())
                {
                    var imageRenderRect = SKRect.Create(
                        (128 - imageBorderSize.Width) / 2,
                        (128 - imageBorderSize.Height) / 2,
                        imageBorderSize.Width,
                        imageBorderSize.Height);

                    // draw image
                    canvas.DrawImage(image, imageRenderRect, imagePaint);
                }
            }
        }
    }
}
