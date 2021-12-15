using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using Nito.Disposables;

namespace Anything.FFmpeg;

public sealed unsafe class VideoStreamDecoder : SingleDisposable<object?>
{
    private const int ContextBufferSize = 4096;
    private readonly AVCodec* _codec;

    private readonly AVCodecContext* _codecContext;
    private readonly AVFormatContext* _formatContext;
    private readonly AVFrame* _frame;
    private readonly AVPacket* _packet;
    private readonly AVFrame* _receivedFrame;
    private readonly int _streamIndex;

    private readonly Stream _videoStream;
    private GCHandle _handle;
    private byte[] _managedContextBuffer;

    public VideoStreamDecoder(Stream videoStream, AVHWDeviceType hwDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        : base(null)
    {
        _videoStream = videoStream;
        var formatContext = ffmpeg.avformat_alloc_context();

        _managedContextBuffer = new byte[ContextBufferSize];
        _handle = GCHandle.Alloc(this, GCHandleType.Normal);

        avio_alloc_context_read_packet readPacketCallback = ReadPacketCallback;
        avio_alloc_context_seek seekCallback = SeekCallback;

        // gets freed by libavformat when closing the input
        var contextBuffer = (byte*)ffmpeg.av_malloc(ContextBufferSize);

        formatContext->pb = ffmpeg.avio_alloc_context(
            contextBuffer,
            ContextBufferSize,
            0,
            (void*)GCHandle.ToIntPtr(_handle),
            readPacketCallback,
            null,
            seekCallback);

        ffmpeg.avformat_open_input(&formatContext, "<no file>", null, null).ThrowExceptionIfError();
        ffmpeg.avformat_find_stream_info(formatContext, null).ThrowExceptionIfError();
        AVCodec* codec = null;
        _streamIndex = ffmpeg
            .av_find_best_stream(formatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0)
            .ThrowExceptionIfError();
        _codecContext = ffmpeg.avcodec_alloc_context3(codec);
        if (hwDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            ffmpeg.av_hwdevice_ctx_create(&_codecContext->hw_device_ctx, hwDeviceType, null, null, 0)
                .ThrowExceptionIfError();
        }

        ffmpeg.avcodec_parameters_to_context(_codecContext, formatContext->streams[_streamIndex]->codecpar)
            .ThrowExceptionIfError();
        ffmpeg.avcodec_open2(_codecContext, codec, null).ThrowExceptionIfError();

        _codec = codec;

        _packet = ffmpeg.av_packet_alloc();
        _frame = ffmpeg.av_frame_alloc();
        _receivedFrame = ffmpeg.av_frame_alloc();
        _formatContext = formatContext;
    }

    public string CodecName => ffmpeg.avcodec_get_name(_codec->id);

    public int FrameWidth => _codecContext->width;

    public int FrameHeight => _codecContext->height;

    public AVPixelFormat PixelFormat => _codecContext->pix_fmt;

    public long Duration => _formatContext->duration;

    private static int ReadPacketCallback(void* opaque, byte* bufferPtr, int bufferSize)
    {
        var handle = GCHandle.FromIntPtr((IntPtr)opaque);
        if (!handle.IsAllocated || handle.Target is not VideoStreamDecoder decoder)
        {
            return 0;
        }

        if (bufferSize != decoder._managedContextBuffer.Length)
        {
            decoder._managedContextBuffer = new byte[bufferSize];
        }

        var bytesRead = decoder._videoStream.Read(decoder._managedContextBuffer, 0, bufferSize);
        Marshal.Copy(decoder._managedContextBuffer, 0, (IntPtr)bufferPtr, bytesRead);
        return bytesRead;
    }

    private static long SeekCallback(void* opaque, long offset, int whence)
    {
        var handle = GCHandle.FromIntPtr((IntPtr)opaque);
        if (!handle.IsAllocated || handle.Target is not VideoStreamDecoder decoder)
        {
            return -1;
        }

        if (!decoder._videoStream.CanSeek)
        {
            throw new InvalidOperationException("Tried seeking on a video sourced by a non-seekable stream.");
        }

        switch (whence)
        {
            case StdIo.SeekCur:
                decoder._videoStream.Seek(offset, SeekOrigin.Current);
                break;

            case StdIo.SeekEnd:
                decoder._videoStream.Seek(offset, SeekOrigin.End);
                break;

            case StdIo.SeekSet:
                decoder._videoStream.Seek(offset, SeekOrigin.Begin);
                break;

            case ffmpeg.AVSEEK_SIZE:
                return decoder._videoStream.Length;

            default:
                return -1;
        }

        return decoder._videoStream.Position;
    }

    protected override void Dispose(object? context)
    {
        var pFrame = _frame;
        ffmpeg.av_frame_free(&pFrame);

        var pReceivedFrame = _receivedFrame;
        ffmpeg.av_frame_free(&pReceivedFrame);

        var pPacket = _packet;
        ffmpeg.av_packet_free(&pPacket);

        ffmpeg.avcodec_close(_codecContext);
        var pFormatContext = _formatContext;
        ffmpeg.avformat_close_input(&pFormatContext);

        _handle.Free();
    }

    public bool TryDecodeNextFrame(out AVFrame frame)
    {
        ffmpeg.av_frame_unref(_frame);
        ffmpeg.av_frame_unref(_receivedFrame);
        int error;

        do
        {
            try
            {
                do
                {
                    ffmpeg.av_packet_unref(_packet);
                    error = ffmpeg.av_read_frame(_formatContext, _packet);

                    if (error == ffmpeg.AVERROR_EOF)
                    {
                        frame = *_frame;
                        return false;
                    }

                    error.ThrowExceptionIfError();
                }
                while (_packet->stream_index != _streamIndex);

                ffmpeg.avcodec_send_packet(_codecContext, _packet).ThrowExceptionIfError();
            }
            finally
            {
                ffmpeg.av_packet_unref(_packet);
            }

            error = ffmpeg.avcodec_receive_frame(_codecContext, _frame);
        }
        while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

        error.ThrowExceptionIfError();

        if (_codecContext->hw_device_ctx != null)
        {
            ffmpeg.av_hwframe_transfer_data(_receivedFrame, _frame, 0).ThrowExceptionIfError();
            frame = *_receivedFrame;
        }
        else
        {
            frame = *_frame;
        }

        return true;
    }

    public void SeekFrame(long timeStamp, int flags = ffmpeg.AVSEEK_FLAG_BACKWARD)
    {
        var streamTimebase = _formatContext->streams[_streamIndex]->time_base.GetValue();
        ffmpeg.av_seek_frame(_formatContext, _streamIndex, (long)(timeStamp / 1000000.0 / streamTimebase), flags)
            .ThrowExceptionIfError();
        ffmpeg.avcodec_flush_buffers(_codecContext);
    }

    private static class StdIo
    {
        internal const int SeekSet = 0;

        internal const int SeekCur = 1;

        internal const int SeekEnd = 2;
    }
}
