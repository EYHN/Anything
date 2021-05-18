using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace StagingBox.Utils
{
    public static class Performance
    {
        public static Timing StartTiming(string name, ILogger? logger, LogLevel logLevel = LogLevel.Debug)
        {
            Timing timing = new Timing(name, logger, logLevel);
            timing.Start();
            return timing;
        }

        public class Timing : IDisposable
        {
            private readonly Stopwatch _stopWatch = new();

            private readonly ILogger? _logger;

            private readonly string _name;

            private readonly LogLevel _logLevel;

            public Timing(string name, ILogger? logger, LogLevel logLevel = LogLevel.Debug)
            {
                _logger = logger;
                _name = name;
                _logLevel = logLevel;
            }

            public void Start()
            {
                _stopWatch.Start();
            }

            public void Stop()
            {
                _stopWatch.Stop();
            }

            public void Dispose()
            {
                Stop();
                _logger?.Log(_logLevel, _name + " - " + _stopWatch.ElapsedMilliseconds + "ms");
            }
        }
    }
}
