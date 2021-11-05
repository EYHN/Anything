using System;
using System.Collections.Generic;

namespace Anything.Utils.Logging
{
    public interface ILogger
    {
        public LoggerProperties Properties { get; }

        public ILogger WithProperties(LoggerProperties properties);

        public bool IsEnabled(LogLevel level);

        public void Write(
            LogLevel level,
            Exception? exception,
            string messageTemplate,
            IEnumerable<object?> variables);
    }
}
