using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace RealTimeTranslator.Core.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logPath;
        private readonly object _lock = new();
        private readonly int _maxFileSizeBytes;
        private readonly int _maxArchiveFiles;

        public FileLogger(string logPath, int maxFileSizeBytes = 10 * 1024 * 1024, int maxArchiveFiles = 5)
        {
            _logPath = logPath;
            _maxFileSizeBytes = maxFileSizeBytes;
            _maxArchiveFiles = maxArchiveFiles;
            
            var logDirectory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public void LogInformation(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(Exception exception, string message)
        {
            WriteLog("ERROR", $"{message}\nException: {exception}");
        }

        private void WriteLog(string level, string message)
        {
            var logMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
            
            lock (_lock)
            {
                try
                {
                    // Check file size and rotate if necessary
                    if (File.Exists(_logPath))
                    {
                        var fileInfo = new FileInfo(_logPath);
                        if (fileInfo.Length >= _maxFileSizeBytes)
                        {
                            RotateLogFiles();
                        }
                    }

                    // Write the log message
                    File.AppendAllText(_logPath, logMessage);
                }
                catch (Exception ex)
                {
                    // If we can't write to the log file, write to the Windows Event Log
                    try
                    {
                        System.Diagnostics.EventLog.WriteEntry(
                            "RealTimeTranslator",
                            $"Failed to write to log file: {ex.Message}\nOriginal message: {logMessage}",
                            System.Diagnostics.EventLogEntryType.Error);
                    }
                    catch
                    {
                        // If all logging fails, we can't do much more
                    }
                }
            }
        }

        private void RotateLogFiles()
        {
            // Delete the oldest log file if it exists
            string oldestLogFile = $"{_logPath}.{_maxArchiveFiles}";
            if (File.Exists(oldestLogFile))
            {
                File.Delete(oldestLogFile);
            }

            // Rotate existing log files
            for (int i = _maxArchiveFiles - 1; i >= 1; i--)
            {
                string currentFile = $"{_logPath}.{i}";
                string nextFile = $"{_logPath}.{i + 1}";
                if (File.Exists(currentFile))
                {
                    File.Move(currentFile, nextFile);
                }
            }

            // Move current log file
            if (File.Exists(_logPath))
            {
                File.Move(_logPath, $"{_logPath}.1");
            }
        }

        public void Flush()
        {
            // No need to implement for file-based logging
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
 