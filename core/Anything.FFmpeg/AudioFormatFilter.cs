using System.Collections.Generic;
using FFmpeg.AutoGen;

namespace Anything.FFmpeg;

public class AudioFormatFilter : AudioFilter
{
    private string _args;

    public AudioFormatFilter(string args, MediaStream sourceStream, IEnumerator<AVFrameRef> source)
        : base(sourceStream, source)
    {
        _args = args;
    }

    public override unsafe void LinkFilterGraph(AVFilterGraph* filterGraph, AVFilterContext* src, AVFilterContext* dst)
    {
        var aformatFilter = ffmpeg.avfilter_get_by_name("aformat");
        if (aformatFilter == null)
        {
            throw new FFmpegException("Could not find aformat filter");
        }

        AVFilterContext* aformat;
        ffmpeg.avfilter_graph_create_filter(
                &aformat,
                aformatFilter,
                "aformat",
                _args,
                null,
                filterGraph)
            .ThrowExceptionIfError();

        ffmpeg.avfilter_link(src, 0, aformat, 0);
        ffmpeg.avfilter_link(aformat, 0, dst, 0);
    }
}
