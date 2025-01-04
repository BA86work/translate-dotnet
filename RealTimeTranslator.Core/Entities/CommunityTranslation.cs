using System;

namespace RealTimeTranslator.Core.Entities
{
    public class CommunityTranslation
    {
        public int Id { get; set; }
        public required string SourceText { get; set; }
        public required string TranslatedText { get; set; }
        public required string SourceLanguage { get; set; }
        public required string TargetLanguage { get; set; }
        public required string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Votes { get; set; }
    }
} 