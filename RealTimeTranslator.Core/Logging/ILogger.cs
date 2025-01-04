namespace RealTimeTranslator.Core.Logging
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(Exception exception, string message);
        void Flush();
    }
} 