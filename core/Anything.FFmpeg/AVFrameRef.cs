using System;
using FFmpeg.AutoGen;

namespace Anything.FFmpeg;

public unsafe struct AVFrameRef : IEquatable<AVFrameRef>
{
    public AVFrame* Value { get; set; } = null;

    public override bool Equals(object? obj)
    {
        if (obj is AVFrameRef other)
        {
            return Equals(other);
        }

        return base.Equals(obj);
    }

    public bool Equals(AVFrameRef other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return new IntPtr(Value).ToInt32();
    }

    public static bool operator ==(AVFrameRef left, AVFrameRef right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AVFrameRef left, AVFrameRef right)
    {
        return !(left == right);
    }
}
