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

        public class Timing : Disposable
        {
            private readonly ILogger? _logger;

            private readonly LogLevel _logLevel;

            private readonly string _name;
            private readonly Stopwatch _stopWatch = new();

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

            protected override void DisposeManaged()
            {
                base.DisposeManaged();

                Stop();
                _logger?.Log(_logLevel, _name + " - " + _stopWatch.ElapsedMilliseconds + "ms");
            }
        }
    }
}
