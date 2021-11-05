using System;

namespace Anything.Utils.Logging
{
    public static class LoggerExtensions
    {
        public static void Write(this ILogger logger, LogLevel level, string messageTemplate, params object?[] variables)
        {
            logger.Write(level, null, messageTemplate, variables);
        }

        public static void Verbose(this ILogger logger, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Verbose, messageTemplate, variables);
        }

        public static void Verbose(this ILogger logger, Exception exception, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Verbose, exception, messageTemplate, variables);
        }

        public static void Debug(this ILogger logger, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Debug, messageTemplate, variables);
        }

        public static void Debug(this ILogger logger, Exception exception, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Debug, exception, messageTemplate, variables);
        }

        public static void Information(this ILogger logger, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Information, messageTemplate, variables);
        }

        public static void Information(
            this ILogger logger,
            Exception exception,
            string messageTemplate,
            params object?[] variables)
        {
            logger.Write(LogLevel.Information, exception, messageTemplate, variables);
        }

        public static void Warning(this ILogger logger, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Warning, messageTemplate, variables);
        }

        public static void Warning(this ILogger logger, Exception exception, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Warning, exception, messageTemplate, variables);
        }

        public static void Error(this ILogger logger, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Error, messageTemplate, variables);
        }

        public static void Error(this ILogger logger, Exception exception, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Error, exception, messageTemplate, variables);
        }

        public static void Fatal(this ILogger logger, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Fatal, messageTemplate, variables);
        }

        public static void Fatal(this ILogger logger, Exception exception, string messageTemplate, params object?[] variables)
        {
            logger.Write(LogLevel.Fatal, exception, messageTemplate, variables);
        }

        public static ILogger WithType(this ILogger logger, Type type)
        {
            return logger.WithProperties(logger.Properties with { SourceType = type });
        }

        public static ILogger WithType<T>(this ILogger logger)
        {
            return logger.WithProperties(logger.Properties with { SourceType = typeof(T) });
        }
    }
}
