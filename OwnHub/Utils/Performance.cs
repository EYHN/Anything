using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OwnHub.Utils
{
    public static class Performance
    {
        public static Timing StartTiming(string name, ILogger? logger, LogLevel logLevel = LogLevel.Debug)
        {
            Timing timing = new Timing(name, logger, logLevel);
            timing.Start();
            return timing;
        }
        
        public class Timing: IDisposable
        {
            private readonly Stopwatch stopWatch = new Stopwatch();
            private readonly ILogger? logger;
            private readonly string name;
            private readonly LogLevel logLevel;

            public Timing(string name, ILogger? logger, LogLevel logLevel = LogLevel.Debug)
            {
                this.logger = logger;
                this.name = name;
                this.logLevel = logLevel;
            }

            public void Start()
            {
                stopWatch.Start();
            }

            public void Stop()
            {
                stopWatch.Stop();
            }

            public void Dispose()
            {
                Stop();
                logger?.Log(logLevel, name + " - " + stopWatch.ElapsedMilliseconds + "ms");
            }
        }
    }
}