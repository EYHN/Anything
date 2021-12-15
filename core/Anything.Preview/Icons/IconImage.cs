using System;

namespace Anything.Preview.Icons;

public record IconImage(
    string ImageFormat,
    int Size,
    ReadOnlyMemory<byte> Data);
