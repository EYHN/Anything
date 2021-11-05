using System;

namespace Anything.Utils.Logging
{
    public record LoggerProperties
    {
        public Type? SourceType { get; init; }
    }
}
