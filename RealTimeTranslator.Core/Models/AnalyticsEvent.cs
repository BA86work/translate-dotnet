using System;

namespace RealTimeTranslator.Core.Models;

public class AnalyticsEvent
{
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
} 