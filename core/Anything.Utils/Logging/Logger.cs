using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Anything.Utils.Logging
{
    public sealed class Logger : ILogger
    {
        private readonly ILoggerBackend _backend;
        private readonly LogLevel _minimumLevel;

        public Logger(ILoggerBackend backend, LogLevel minimumLevel = LogLevel.Verbose, LoggerProperties? context = null)
        {
            _minimumLevel = minimumLevel;
            _backend = backend;
            Properties = context ?? new LoggerProperties();
        }

        public static Logger Slient => new(new SilentBackend(), LogLevel.Fatal);

        public LoggerProperties Properties { get; }

        public ILogger WithProperties(LoggerProperties properties)
        {
            return new Logger(_backend, _minimumLevel, properties);
        }

        public bool IsEnabled(LogLevel level)
        {
            if ((int)level < (int)_minimumLevel)
            {
                return false;
            }

            return true;
        }

        public void Write(LogLevel level, Exception? exception, string messageTemplate, IEnumerable<object?> variables)
        {
            if (IsEnabled(level))
            {
                _backend.Write(new LogMessage(
                    DateTimeOffset.Now,
                    level,
                    exception,
                    messageTemplate,
                    variables.ToImmutableArray(),
                    Properties));
            }
        }
    }
}
