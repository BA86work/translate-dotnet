using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RealTimeTranslator.Core.Models;
using RealTimeTranslator.Data;

namespace RealTimeTranslator.Core.Analytics;

public class AnalyticsService
{
    private readonly ILogger<AnalyticsService> _logger;
    private readonly TranslatorDbContext _dbContext;
    private readonly ConcurrentQueue<AnalyticsEvent> _eventQueue;

    public AnalyticsService(ILogger<AnalyticsService> logger, TranslatorDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _eventQueue = new ConcurrentQueue<AnalyticsEvent>();
    }

    public void TrackEvent(string eventType, string? eventData = null)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            EventType = eventType,
            EventData = eventData ?? string.Empty,
            Timestamp = DateTime.UtcNow
        };

        _eventQueue.Enqueue(analyticsEvent);
    }

    public async Task FlushEventsAsync()
    {
        while (_eventQueue.TryDequeue(out var analyticsEvent))
        {
            try
            {
                // Save to database
                _logger.LogInformation("Saving analytics event: {EventType}", analyticsEvent.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save analytics event");
            }
        }
    }
} 