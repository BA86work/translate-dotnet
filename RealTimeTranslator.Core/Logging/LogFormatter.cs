using System;
using Microsoft.Extensions.Logging;

namespace RealTimeTranslator.Core.Logging;

public static class LogFormatter
{
    public static string FormatLogMessage(LogLevel logLevel, string message, Exception? exception = null)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var exceptionMessage = exception != null ? $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}" : string.Empty;
        
        return $"[{timestamp}] [{logLevel}] {message}{exceptionMessage}";
    }
} 