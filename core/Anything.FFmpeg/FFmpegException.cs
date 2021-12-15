using System;

namespace Anything.FFmpeg;

public class FFmpegException : Exception
{
    public FFmpegException(string? message = null)
        : base(message)
    {
    }
}
