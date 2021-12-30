using System;
using System.Collections;
using System.Collections.Generic;
using FFmpeg.AutoGen;
using Nito.Disposables;

namespace Anything.FFmpeg;

public sealed unsafe class StreamDecoder : SingleDisposable<object?>, IEnumerator<AVFrameRef>
{
    private readonly AVFormatContext* _formatContext;
    private readonly AVStream* _stream;
    private readonly int _streamIndex;
    private readonly AVCodec* _codec;

    private readonly AVCodecContext* _codecContext;
    private readonly AVFrame* _frame;
    private readonly AVPacket* _packet;
    private readonly AVFrame* _receivedFrame;

    public AVFrameRef Current { get; private set; }

    object IEnumerator.Current => Current;

    public StreamDecoder(
        AVFormatContext* formatContext,
        int streamIndex,
        AVCodec* codec,
        AVHWDeviceType hwDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        : base(null)
    {
        _streamIndex = streamIndex;
        _formatContext = formatContext;
        _stream = formatContext->streams[streamIndex];

        _codecContext = ffmpeg.avcodec_alloc_context3(codec);
        if (hwDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            ffmpeg.av_hwdevice_ctx_create(&_codecContext->hw_device_ctx, hwDeviceType, null, null, 0)
                .ThrowExceptionIfError();
        }

        ffmpeg.avcodec_parameters_to_context(_codecContext, _stream->codecpar)
            .ThrowExceptionIfError();
        ffmpeg.avcodec_open2(_codecContext, codec, null).ThrowExceptionIfError();
        _codecContext->pkt_timebase = _stream->time_base;

        _codec = codec;

        _packet = ffmpeg.av_packet_alloc();
        _frame = ffmpeg.av_frame_alloc();
        _receivedFrame = ffmpeg.av_frame_alloc();
    }

    public string CodecName => ffmpeg.avcodec_get_name(_codec->id);

    public int FrameWidth => _codecContext->width;

    public int FrameHeight => _codecContext->height;

    public AVPixelFormat PixelFormat => _codecContext->pix_fmt;

    public double Duration => (double)_stream->duration / ffmpeg.AV_TIME_BASE;

    protected override void Dispose(object? context)
    {
        var pFrame = _frame;
        ffmpeg.av_frame_free(&pFrame);

        var pReceivedFrame = _receivedFrame;
        ffmpeg.av_frame_free(&pReceivedFrame);

        var pPacket = _packet;
        ffmpeg.av_packet_free(&pPacket);

        ffmpeg.avcodec_close(_codecContext);
    }

    public bool MoveNext()
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
                        break;
                    }

                    error.ThrowExceptionIfError();
                }
                while (_packet->stream_index != _streamIndex);

                error = ffmpeg.avcodec_send_packet(_codecContext, error == ffmpeg.AVERROR_EOF ? null : _packet);

                if (error != ffmpeg.AVERROR_EOF)
                {
                    error.ThrowExceptionIfError();
                }
            }
            finally
            {
                ffmpeg.av_packet_unref(_packet);
            }

            error = ffmpeg.avcodec_receive_frame(_codecContext, _frame);
        }
        while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

        if (error == ffmpeg.AVERROR_EOF)
        {
            Current = default;
            return false;
        }

        error.ThrowExceptionIfError();

        if (_codecContext->hw_device_ctx != null)
        {
            ffmpeg.av_hwframe_transfer_data(_receivedFrame, _frame, 0).ThrowExceptionIfError();
            Current = new AVFrameRef { Value = _receivedFrame };
        }
        else
        {
            Current = new AVFrameRef { Value = _frame };
        }

        return true;
    }

    public void SeekFrame(double seconds, int flags = ffmpeg.AVSEEK_FLAG_BACKWARD)
    {
        var streamTimebase = _formatContext->streams[_streamIndex]->time_base.GetValue();
        ffmpeg.av_seek_frame(_formatContext, _streamIndex, (long)(seconds * ffmpeg.AV_TIME_BASE / streamTimebase), flags)
            .ThrowExceptionIfError();
        ffmpeg.avcodec_flush_buffers(_codecContext);
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }
}
