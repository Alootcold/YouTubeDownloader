using System.IO;
using System.Text.Json;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Services;

public class HistoryService
{
    private static readonly Lazy<HistoryService> _instance = new(() => new HistoryService());
    public static HistoryService Instance => _instance.Value;

    private readonly string _historyFile;
    private List<DownloadHistory> _histories = new();

    public event EventHandler<DownloadHistory>? HistoryAdded;
    public event EventHandler<string>? HistoryRemoved;

    private HistoryService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YouTubeDownloader");
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        _historyFile = Path.Combine(appDataPath, "history.json");
        LoadHistory();
    }

    private void LoadHistory()
    {
        if (!File.Exists(_historyFile))
        {
            _histories = new List<DownloadHistory>();
            return;
        }

        try
        {
            var json = File.ReadAllText(_historyFile);
            _histories = JsonSerializer.Deserialize<List<DownloadHistory>>(json) ?? new List<DownloadHistory>();
        }
        catch
        {
            _histories = new List<DownloadHistory>();
        }
    }

    public async Task SaveHistoryAsync()
    {
        var json = JsonSerializer.Serialize(_histories, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_historyFile, json);
    }

    public async Task AddHistoryAsync(DownloadHistory history)
    {
        _histories.Insert(0, history);

        var existing = _histories.FirstOrDefault(h => h.Id == history.Id);
        if (existing != null)
        {
            _histories.Remove(existing);
            _histories.Insert(0, history);
        }

        await SaveHistoryAsync();
        HistoryAdded?.Invoke(this, history);
    }

    public async Task RemoveHistoryAsync(string id)
    {
        var history = _histories.FirstOrDefault(h => h.Id == id);
        if (history != null)
        {
            _histories.Remove(history);
            await SaveHistoryAsync();
            HistoryRemoved?.Invoke(this, id);
        }
    }

    public async Task ClearHistoryAsync()
    {
        _histories.Clear();
        await SaveHistoryAsync();
    }

    public List<DownloadHistory> GetAllHistory()
    {
        return _histories.ToList();
    }

    public List<DownloadHistory> GetHistoryByStatus(DownloadStatus status)
    {
        return _histories.Where(h => h.Status == status).ToList();
    }

    public DownloadHistory? GetHistoryById(string id)
    {
        return _histories.FirstOrDefault(h => h.Id == id);
    }
}