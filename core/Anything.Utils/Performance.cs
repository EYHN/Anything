using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Anything.Utils
{
    public static class Performance
    {
        public static Timing StartTiming(string name, ILogger? logger, LogLevel logLevel = LogLevel.Debug)
        {
            Timing timing = new(name, logger, logLevel);
            timing.Start();
            return timing;
        }

        public class Timing : IDisposable
        {
            private readonly ILogger? _logger;

            private readonly LogLevel _logLevel;

            private readonly string _name;
            private readonly Stopwatch _stopWatch = new();
            private bool _disposed;

            public Timing(string name, ILogger? logger, LogLevel logLevel = LogLevel.Debug)
            {
                _logger = logger;
                _name = name;
                _logLevel = logLevel;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Start()
            {
                _stopWatch.Start();
            }

            public void Stop()
            {
                _stopWatch.Stop();
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        Stop();
                        _logger?.Log(_logLevel, _name + " - " + _stopWatch.ElapsedMilliseconds + "ms");
                    }

                    _disposed = true;
                }
            }

            ~Timing()
            {
                Dispose(false);
            }
        }
    }
}
