using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using YouTubeDownloader.Models;
using YouTubeDownloader.Services;

namespace YouTubeDownloader.ViewModels;

public class DownloadListViewModel : ViewModelBase
{
    private readonly YouTubeService _youTubeService;
    private readonly HistoryService _historyService;
    private readonly DispatcherQueue _dispatcherQueue;

    public DownloadListViewModel()
    {
        _youTubeService = YouTubeService.Instance;
        _historyService = HistoryService.Instance;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        PauseCommand = new RelayCommand(PauseTask, CanPauseTask);
        ResumeCommand = new RelayCommand(ResumeTask, CanResumeTask);
        CancelCommand = new RelayCommand(CancelTask, CanCancelTask);
        RemoveCommand = new RelayCommand(RemoveTask);
    }

    public ObservableCollection<DownloadTask> DownloadTasks { get; } = new();

    public RelayCommand PauseCommand { get; }
    public RelayCommand ResumeCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand RemoveCommand { get; }

    private bool _isEmpty = true;
    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetProperty(ref _isEmpty, value);
    }

    private DownloadTask? _selectedTask;
    public DownloadTask? SelectedTask
    {
        get => _selectedTask;
        set => SetProperty(ref _selectedTask, value);
    }

    public void AddTask(DownloadTask task)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            DownloadTasks.Add(task);
            IsEmpty = DownloadTasks.Count == 0;
        });
    }

    public void RemoveTask(object? parameter)
    {
        if (parameter is DownloadTask task)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                DownloadTasks.Remove(task);
                IsEmpty = DownloadTasks.Count == 0;
            });
        }
    }

    public void UpdateTask(DownloadTask task)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var existing = DownloadTasks.FirstOrDefault(t => t.Id == task.Id);
            if (existing != null)
            {
                var index = DownloadTasks.IndexOf(existing);
                DownloadTasks[index] = task;
            }
        });
    }

    private void PauseTask(object? parameter)
    {
        if (parameter is DownloadTask task)
        {
            task.Status = DownloadStatus.Paused;
            UpdateTask(task);
        }
    }

    private bool CanPauseTask(object? parameter)
    {
        return parameter is DownloadTask task && task.Status == DownloadStatus.Downloading;
    }

    private void ResumeTask(object? parameter)
    {
        if (parameter is DownloadTask task)
        {
            task.Status = DownloadStatus.Downloading;
            UpdateTask(task);
        }
    }

    private bool CanResumeTask(object? parameter)
    {
        return parameter is DownloadTask task && task.Status == DownloadStatus.Paused;
    }

    private void CancelTask(object? parameter)
    {
        if (parameter is DownloadTask task)
        {
            _youTubeService.CancelDownload(task.Id);
            task.Status = DownloadStatus.Cancelled;
            UpdateTask(task);
        }
    }

    private bool CanCancelTask(object? parameter)
    {
        return parameter is DownloadTask task &&
               (task.Status == DownloadStatus.Downloading || task.Status == DownloadStatus.Waiting);
    }
}