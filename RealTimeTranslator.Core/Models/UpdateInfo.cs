using System;

namespace RealTimeTranslator.Core.Models;

public class UpdateInfo
{
    public Version Version { get; set; } = new Version();
    public string DownloadUrl { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime ReleaseDate { get; set; }
} 