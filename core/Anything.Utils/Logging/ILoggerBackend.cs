namespace Anything.Utils.Logging
{
    public interface ILoggerBackend
    {
        public void Write(LogMessage message);
    }
}
