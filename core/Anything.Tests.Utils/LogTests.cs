using Anything.Utils.Logging;
using NUnit.Framework;

namespace Anything.Tests.Utils
{
    public class LogTests
    {
        [Test]
        public void LogTest()
        {
            var logger = new Logger(new SerilogCommandLineLoggerBackend());

            logger.WithType<LogTests>().Debug("hello world");
            logger.Information("message here");
            logger.WithType<LogTests>().Verbose("my name is cooooooooool");
        }
    }
}
