using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RealTimeTranslator.Core.Monitoring;

public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly Stopwatch _stopwatch;

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger;
        _stopwatch = new Stopwatch();
    }

    public IDisposable MeasureOperation(string operationName)
    {
        return new OperationTimer(_logger, operationName);
    }

    public void LogSystemMetrics()
    {
        var process = Process.GetCurrentProcess();
        var workingSet = process.WorkingSet64 / 1024.0 / 1024.0; // Convert to MB
        var cpuTime = process.TotalProcessorTime;

        _logger.LogInformation(
            "System Metrics - Memory: {WorkingSet:F2}MB, CPU Time: {CpuTime}",
            workingSet,
            cpuTime);
    }

    private class OperationTimer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        public OperationTimer(ILogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.LogInformation(
                "Operation {OperationName} completed in {ElapsedMilliseconds}ms",
                _operationName,
                _stopwatch.ElapsedMilliseconds);
        }
    }
} 