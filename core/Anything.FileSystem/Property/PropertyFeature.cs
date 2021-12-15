using System;

namespace Anything.FileSystem.Property;

[Flags]
public enum PropertyFeature
{
    None = 0,
    AutoDeleteWhenFileUpdate = 1
}
