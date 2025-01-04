using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RealTimeTranslator.Core.Models;

public class SettingsService
{
    private readonly string _settingsPath;
    private readonly ILogger _logger;
    private UserSettings _currentSettings;
    private readonly object _lock = new();

    public event EventHandler<UserSettings> SettingsChanged;

    public SettingsService(string settingsPath, ILogger logger)
    {
        _settingsPath = settingsPath;
        _logger = logger;
        LoadSettings();
    }

    public UserSettings GetSettings()
    {
        lock (_lock)
        {
            return _currentSettings;
        }
    }

    public async Task SaveSettingsAsync(UserSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            lock (_lock)
            {
                File.WriteAllText(_settingsPath, json);
                _currentSettings = settings;
            }

            SettingsChanged?.Invoke(this, settings);
            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            throw;
        }
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _currentSettings = JsonSerializer.Deserialize<UserSettings>(json);
            }
            else
            {
                _currentSettings = GetDefaultSettings();
                SaveSettingsAsync(_currentSettings).Wait();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings, using defaults");
            _currentSettings = GetDefaultSettings();
        }
    }

    private UserSettings GetDefaultSettings()
    {
        return new UserSettings
        {
            SourceLanguage = "en",
            TargetLanguage = "ja",
            AutoTranslate = true,
            ShowOriginalText = true,
            OverlayOpacity = 0.8,
            Hotkeys = new Dictionary<string, string>
            {
                { "Translate", "Ctrl+T" },
                { "CaptureArea", "Ctrl+Shift+C" }
            },
            EnableCommunityTranslations = true,
            CacheRetentionDays = 30,
            Theme = "Dark"
        };
    }
} 