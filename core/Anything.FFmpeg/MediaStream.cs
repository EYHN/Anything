using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace Anything.FFmpeg;

/// <summary>
/// Associate one AVStream pointer.
/// </summary>
public unsafe class MediaStream
{
    public AVFormatContext* RawFormatContext { get; }

    public int StreamIndex { get; }

    public AVStream* RawStream { get; }

    private AVCodec* _cachedRawDecodeCodec;

    public AVMediaType MediaType => RawStream->codecpar->codec_type;

    public AVSampleFormat SampleFormat => MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO
        ? (AVSampleFormat)RawStream->codecpar->format
        : AVSampleFormat.AV_SAMPLE_FMT_NONE;

    public int SampleRate => MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO ? RawStream->codecpar->sample_rate : default;

    public int Channels => MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO ? RawStream->codecpar->channels : default;

    public string? ChannelLayout
    {
        get
        {
            if (MediaType != AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                return null;
            }

            var layout = RawStream->codecpar->channel_layout;
            if (layout == 0)
            {
                return null;
            }

            var stringBuffer = stackalloc byte[64];

            ffmpeg.av_get_channel_layout_string(stringBuffer, 64, -1, layout);
            return Marshal.PtrToStringUTF8((IntPtr)stringBuffer, 64);
        }
    }

    public AVRational TimeBase => RawStream->time_base;

    public long DurationTs => RawStream->duration;

    public double Duration => (double)RawStream->duration * RawStream->time_base.num / RawStream->time_base.den;

    public AVCodec* RawDecodeCodec
    {
        get
        {
            if (_cachedRawDecodeCodec != null)
            {
                return _cachedRawDecodeCodec;
            }

            _cachedRawDecodeCodec = ffmpeg.avcodec_find_decoder(RawStream->codecpar->codec_id);
            return _cachedRawDecodeCodec;
        }
    }

    public MediaStream(AVFormatContext* formatContext, int streamIndex, AVStream* rawStream, AVCodec* decodeCodec = null)
    {
        RawFormatContext = formatContext;
        StreamIndex = streamIndex;
        RawStream = rawStream;
        _cachedRawDecodeCodec = decodeCodec;
    }

    public UnmanagedMemoryStream ReadAttachedPicture()
    {
        var packet = RawStream->attached_pic;
        return new UnmanagedMemoryStream(packet.data, packet.size);
    }

    public StreamDecoder CreateStreamDecoder()
    {
        return new StreamDecoder(RawFormatContext, StreamIndex, RawDecodeCodec);
    }
}
