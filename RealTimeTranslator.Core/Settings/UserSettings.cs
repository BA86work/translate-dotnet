public class UserSettings
{
    public string SourceLanguage { get; set; }
    public string TargetLanguage { get; set; }
    public bool AutoTranslate { get; set; }
    public bool ShowOriginalText { get; set; }
    public double OverlayOpacity { get; set; }
    public Dictionary<string, string> Hotkeys { get; set; }
    public bool EnableCommunityTranslations { get; set; }
    public int CacheRetentionDays { get; set; }
    public string Theme { get; set; }
} 