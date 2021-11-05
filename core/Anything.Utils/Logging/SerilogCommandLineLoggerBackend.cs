using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Anything.Utils.Logging
{
    public class SerilogCommandLineLoggerBackend : ILoggerBackend
    {
        private readonly Serilog.ILogger _innerLogger;

        public SerilogCommandLineLoggerBackend()
        {
            _innerLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(
                    applyThemeToRedirectedOutput: true,
                    outputTemplate: "{Level:u1} [{Timestamp:HH:mm:ss}]: {Message:lj} <{SourceType}>{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }

        public void Write(LogMessage message)
        {
            var level = ConvertLogLevel(message.LogLevel);
            if (_innerLogger.IsEnabled(level))
            {
                _innerLogger
                    .ForContext(new LoggerContextEnricher(message.Properties))
                    .Write(level, message.Exception, message.MessageTemplate, message.Variables.ToArray());
            }
        }

        public static LogEventLevel ConvertLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Verbose => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Fatal => LogEventLevel.Fatal,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }

        private class LoggerContextEnricher : ILogEventEnricher
        {
            private static readonly ConditionalWeakTable<LoggerProperties, List<LogEventProperty>> _cache = new();
            private readonly LoggerProperties _properties;

            public LoggerContextEnricher(LoggerProperties properties)
            {
                _properties = properties;
            }

            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                List<LogEventProperty>? properties;

                if (!_cache.TryGetValue(_properties, out properties))
                {
                    properties = new List<LogEventProperty>();
                    if (_properties.SourceType != null)
                    {
                        properties.Add(propertyFactory.CreateProperty("SourceType", _properties.SourceType));
                    }

                    _cache.AddOrUpdate(_properties, properties);
                }

                foreach (var property in properties)
                {
                    logEvent.AddPropertyIfAbsent(property);
                }
            }
        }
    }
}
