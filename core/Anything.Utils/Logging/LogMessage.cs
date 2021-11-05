using System;
using System.Collections.Generic;

namespace Anything.Utils.Logging
{
    public record LogMessage(
        DateTimeOffset Timestamp,
        LogLevel LogLevel,
        Exception? Exception,
        string MessageTemplate,
        IReadOnlyCollection<object> Variables,
        LoggerProperties Properties);
}
