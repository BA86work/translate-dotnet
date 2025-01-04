using System;

namespace RealTimeTranslator.Data.Entities;

public class TranslationCache
{
    public int Id { get; set; }
    public required string SourceText { get; set; }
    public required string TranslatedText { get; set; }
    public required string FromLanguage { get; set; }
    public required string ToLanguage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
} 