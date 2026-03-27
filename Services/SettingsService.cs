using System.IO;
using System.Text.Json;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Services;

public class SettingsService
{
    private static readonly Lazy<SettingsService> _instance = new(() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    private readonly string _settingsFile;
    private AppSettings _settings = new();

    public event EventHandler<AppSettings>? SettingsChanged;

    private SettingsService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YouTubeDownloader");
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        _settingsFile = Path.Combine(appDataPath, "settings.json");
        LoadSettings();
    }

    private void LoadSettings()
    {
        if (!File.Exists(_settingsFile))
        {
            _settings = new AppSettings();
            return;
        }

        try
        {
            var json = File.ReadAllText(_settingsFile);
            _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            _settings = new AppSettings();
        }
    }

    public async Task SaveSettingsAsync()
    {
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFile, json);
        SettingsChanged?.Invoke(this, _settings);
    }

    public AppSettings GetSettings()
    {
        return _settings;
    }

    public async Task UpdateSettingsAsync(Action<AppSettings> updateAction)
    {
        updateAction(_settings);
        await SaveSettingsAsync();
    }

    public string GetDownloadPath()
    {
        if (string.IsNullOrEmpty(_settings.DownloadPath))
        {
            _settings.DownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "YouTubeDownloader");
        }
        return _settings.DownloadPath;
    }

    public async Task SetDownloadPathAsync(string path)
    {
        _settings.DownloadPath = path;
        await SaveSettingsAsync();
    }
}