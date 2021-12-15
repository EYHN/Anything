using System;
using MessagePack;

namespace Anything.Preview.Thumbnails;

[MessagePackObject]
public record ThumbnailImage(
    [property: Key(0)] string ImageFormat,
    [property: Key(1)] int Size,
    [property: Key(2)] ReadOnlyMemory<byte> Data);
