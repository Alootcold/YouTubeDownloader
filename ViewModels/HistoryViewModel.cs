using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.UI.Dispatching;
using YouTubeDownloader.Models;
using YouTubeDownloader.Services;

namespace YouTubeDownloader.ViewModels;

public class HistoryViewModel : ViewModelBase
{
    private readonly HistoryService _historyService;
    private readonly DispatcherQueue _dispatcherQueue;

    public HistoryViewModel()
    {
        _historyService = HistoryService.Instance;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _historyService.HistoryAdded += OnHistoryAdded;
        _historyService.HistoryRemoved += OnHistoryRemoved;

        OpenFileCommand = new RelayCommand(OpenFile);
        OpenFolderCommand = new RelayCommand(OpenFolder);
        DeleteRecordCommand = new AsyncRelayCommand(DeleteRecordAsync);
        ClearAllCommand = new AsyncRelayCommand(ClearAllAsync);

        LoadHistory();
    }

    public ObservableCollection<DownloadHistory> HistoryRecords { get; } = new();

    public RelayCommand OpenFileCommand { get; }
    public RelayCommand OpenFolderCommand { get; }
    public AsyncRelayCommand DeleteRecordCommand { get; }
    public AsyncRelayCommand ClearAllCommand { get; }

    private DownloadHistory? _selectedRecord;
    public DownloadHistory? SelectedRecord
    {
        get => _selectedRecord;
        set => SetProperty(ref _selectedRecord, value);
    }

    private bool _isEmpty = true;
    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetProperty(ref _isEmpty, value);
    }

    private void LoadHistory()
    {
        var histories = _historyService.GetAllHistory();
        _dispatcherQueue.TryEnqueue(() =>
        {
            HistoryRecords.Clear();
            foreach (var history in histories)
            {
                HistoryRecords.Add(history);
            }
            IsEmpty = HistoryRecords.Count == 0;
        });
    }

    private void OnHistoryAdded(object? sender, DownloadHistory history)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            HistoryRecords.Insert(0, history);
            IsEmpty = HistoryRecords.Count == 0;
        });
    }

    private void OnHistoryRemoved(object? sender, string id)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var record = HistoryRecords.FirstOrDefault(h => h.Id == id);
            if (record != null)
            {
                HistoryRecords.Remove(record);
                IsEmpty = HistoryRecords.Count == 0;
            }
        });
    }

    private void OpenFile(object? parameter)
    {
        if (parameter is DownloadHistory history && System.IO.File.Exists(history.FilePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = history.FilePath,
                UseShellExecute = true
            });
        }
    }

    private void OpenFolder(object? parameter)
    {
        if (parameter is DownloadHistory history)
        {
            var folder = System.IO.Path.GetDirectoryName(history.FilePath);
            if (!string.IsNullOrEmpty(folder) && System.IO.Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{history.FilePath}\"",
                    UseShellExecute = true
                });
            }
        }
    }

    private async Task DeleteRecordAsync(object? parameter)
    {
        if (parameter is DownloadHistory history)
        {
            await _historyService.RemoveHistoryAsync(history.Id);
        }
    }

    private async Task ClearAllAsync()
    {
        await _historyService.ClearHistoryAsync();
        _dispatcherQueue.TryEnqueue(() =>
        {
            HistoryRecords.Clear();
            IsEmpty = true;
        });
    }

    public void Refresh()
    {
        LoadHistory();
    }
}