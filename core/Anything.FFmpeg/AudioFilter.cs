using System;
using System.Collections;
using System.Collections.Generic;
using FFmpeg.AutoGen;
using Nito.Disposables;

namespace Anything.FFmpeg;

public abstract unsafe class AudioFilter : SingleDisposable<object?>, IEnumerator<AVFrameRef>
{
    private readonly MediaStream _sourceStream;
    private readonly IEnumerator<AVFrameRef> _source;

    private AVFilterGraph* _rawFilterGraph;
    private AVFilterContext* _src;
    private AVFilterContext* _dst;

    public AVFrameRef Current { get; }

    object IEnumerator.Current => Current;

    protected AudioFilter(MediaStream sourceStream, IEnumerator<AVFrameRef> source)
        : base(null)
    {
        _sourceStream = sourceStream;
        _source = source;

        if (sourceStream.MediaType != AVMediaType.AVMEDIA_TYPE_AUDIO)
        {
            throw new ArgumentException("Source must be audio");
        }

        Current = new AVFrameRef { Value = ffmpeg.av_frame_alloc() };
    }

    public void Build()
    {
        _rawFilterGraph = ffmpeg.avfilter_graph_alloc();
        if (_rawFilterGraph == null)
        {
            throw new FFmpegException("Unable to create filter graph.");
        }

        var aBuffer = ffmpeg.avfilter_get_by_name("abuffer");
        if (aBuffer == null)
        {
            throw new FFmpegException("Could not find the abuffer filter.");
        }

        _src = ffmpeg.avfilter_graph_alloc_filter(_rawFilterGraph, aBuffer, "src");
        if (_src == null)
        {
            throw new FFmpegException("Could not allocate the abuffer instance.");
        }

        ffmpeg.av_opt_set(_src, "channel_layout", _sourceStream.ChannelLayout!, ffmpeg.AV_OPT_SEARCH_CHILDREN);
        ffmpeg.av_opt_set(
            _src,
            "sample_fmt",
            ffmpeg.av_get_sample_fmt_name(_sourceStream.SampleFormat),
            ffmpeg.AV_OPT_SEARCH_CHILDREN);
        ffmpeg.av_opt_set_q(_src, "time_base", _sourceStream.TimeBase, ffmpeg.AV_OPT_SEARCH_CHILDREN);
        ffmpeg.av_opt_set_int(_src, "sample_rate", _sourceStream.SampleRate, ffmpeg.AV_OPT_SEARCH_CHILDREN);

        ffmpeg.avfilter_init_str(_src, null).ThrowExceptionIfError();

        var aBufferSink = ffmpeg.avfilter_get_by_name("abuffersink");
        if (aBufferSink == null)
        {
            throw new FFmpegException("Could not find the abuffersink filter.");
        }

        _dst = ffmpeg.avfilter_graph_alloc_filter(_rawFilterGraph, aBufferSink, "dst");
        ffmpeg.avfilter_init_str(_dst, null).ThrowExceptionIfError();

        LinkFilterGraph(_rawFilterGraph, _src, _dst);

        ffmpeg.avfilter_graph_config(_rawFilterGraph, null).ThrowExceptionIfError();
    }

    public abstract void LinkFilterGraph(AVFilterGraph* filterGraph, AVFilterContext* src, AVFilterContext* dst);

    public void Send(AVFrame* frame)
    {
        ffmpeg.av_buffersrc_add_frame(_src, frame).ThrowExceptionIfError();
    }

    public bool MoveNext()
    {
        while (true)
        {
            var hasNext = _source.MoveNext();
            if (!hasNext)
            {
                ffmpeg.av_buffersrc_add_frame_flags(_src, null, 0).ThrowExceptionIfError();
            }
            else
            {
                ffmpeg.av_buffersrc_add_frame_flags(_src, _source.Current.Value, 0).ThrowExceptionIfError();
            }

            ffmpeg.av_frame_unref(Current.Value);

            var error = ffmpeg.av_buffersink_get_frame(_dst, Current.Value);

            if (error == 0)
            {
                return true;
            }
            else if (error == ffmpeg.AVERROR_EOF)
            {
                return false;
            }
            else if (error == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                continue;
            }
            else
            {
                error.ThrowExceptionIfError();
                return false;
            }
        }
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(object? context)
    {
        var pFrame = Current.Value;
        ffmpeg.av_frame_free(&pFrame);
    }
}
