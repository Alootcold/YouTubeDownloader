using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using YouTubeDownloader.Models;
using YouTubeDownloader.Services;

namespace YouTubeDownloader.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly YouTubeService _youTubeService;
    private readonly DownloadEngineService _engineService;
    private readonly SettingsService _settingsService;
    private readonly HistoryService _historyService;
    private readonly DispatcherQueue _dispatcherQueue;

    private string _videoUrl = string.Empty;
    private VideoInfo? _videoInfo;
    private DownloadTask? _currentTask;
    private bool _isLoading;
    private bool _isDownloading;
    private double _downloadProgress;
    private string _downloadStatus = "等待中";
    private string _downloadSpeed = string.Empty;
    private string _errorMessage = string.Empty;
    private string _selectedQuality = "best";
    private string _downloadProgressText = "0%";
    private CancellationTokenSource? _cancellationTokenSource;

    public HomeViewModel()
    {
        _youTubeService = YouTubeService.Instance;
        _engineService = DownloadEngineService.Instance;
        _settingsService = SettingsService.Instance;
        _historyService = HistoryService.Instance;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _youTubeService.DownloadProgress += OnDownloadProgress;
        _youTubeService.DownloadCompleted += OnDownloadCompleted;

        ParseCommand = new AsyncRelayCommand(ParseVideoAsync);
        DownloadCommand = new AsyncRelayCommand(StartDownloadAsync, () => VideoInfo != null && !IsDownloading);
        CancelCommand = new RelayCommand(CancelDownload, () => IsDownloading);
        ClearCommand = new RelayCommand(Clear);
    }

    public string VideoUrl
    {
        get => _videoUrl;
        set
        {
            if (SetProperty(ref _videoUrl, value))
            {
                OnPropertyChanged(nameof(CanParse));
            }
        }
    }

    public VideoInfo? VideoInfo
    {
        get => _videoInfo;
        set
        {
            if (SetProperty(ref _videoInfo, value))
            {
                OnPropertyChanged(nameof(HasVideoInfo));
                OnPropertyChanged(nameof(CanDownload));
            }
        }
    }

    public DownloadTask? CurrentTask
    {
        get => _currentTask;
        set => SetProperty(ref _currentTask, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        set
        {
            if (SetProperty(ref _isDownloading, value))
            {
                OnPropertyChanged(nameof(CanParse));
                OnPropertyChanged(nameof(CanDownload));
            }
        }
    }

    public double DownloadProgress
    {
        get => _downloadProgress;
        set
        {
            if (SetProperty(ref _downloadProgress, value))
            {
                DownloadProgressText = $"{value:F1}%";
            }
        }
    }

    public string DownloadProgressText
    {
        get => _downloadProgressText;
        set => SetProperty(ref _downloadProgressText, value);
    }

    public string DownloadStatus
    {
        get => _downloadStatus;
        set => SetProperty(ref _downloadStatus, value);
    }

    public string DownloadSpeed
    {
        get => _downloadSpeed;
        set => SetProperty(ref _downloadSpeed, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string SelectedQuality
    {
        get => _selectedQuality;
        set => SetProperty(ref _selectedQuality, value);
    }

    public bool HasVideoInfo => VideoInfo != null;

    public bool CanParse => !string.IsNullOrWhiteSpace(VideoUrl) && !IsLoading && !IsDownloading;

    public bool CanDownload => VideoInfo != null && !IsLoading && !IsDownloading;

    public ObservableCollection<string> AvailableQualities { get; } = new();

    public AsyncRelayCommand ParseCommand { get; }
    public AsyncRelayCommand DownloadCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand ClearCommand { get; }

    private async Task ParseVideoAsync()
    {
        if (string.IsNullOrWhiteSpace(VideoUrl))
            return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        VideoInfo = null;
        AvailableQualities.Clear();

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            VideoInfo? info = await _youTubeService.GetVideoInfoAsync(VideoUrl, _cancellationTokenSource.Token);

            if (info != null)
            {
                VideoInfo = info;

                AvailableQualities.Clear();
                AvailableQualities.Add("best");
                foreach (string quality in info.AvailableQualities)
                {
                    AvailableQualities.Add(quality);
                }
                SelectedQuality = "best";
            }
            else
            {
                ErrorMessage = "无法解析视频信息，请检查URL是否正确。";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"解析失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async Task StartDownloadAsync()
    {
        if (VideoInfo == null)
            return;

        IsDownloading = true;
        DownloadProgress = 0;
        ErrorMessage = string.Empty;
        DownloadStatus = "准备下载...";

        try
        {
            string downloadPath = Path.Combine(
                _settingsService.GetDownloadPath(),
                $"{SanitizeFileName(VideoInfo.Title)}.mp4"
            );

            DownloadTask task = new DownloadTask
            {
                VideoUrl = VideoUrl,
                VideoInfo = VideoInfo,
                OutputPath = downloadPath,
                SelectedQuality = SelectedQuality,
                SelectedFormat = "mp4"
            };
            CurrentTask = task;

            _cancellationTokenSource = new CancellationTokenSource();
            IProgress<DownloadProgressEventArgs> progress = new Progress<DownloadProgressEventArgs>(args =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    DownloadProgress = args.Progress;
                    DownloadSpeed = args.Speed;
                    DownloadStatus = $"下载中... {args.Progress:F1}%";
                });
            });

            DownloadTask resultTask = await _youTubeService.StartDownloadAsync(task, progress, _cancellationTokenSource.Token);

            if (resultTask.Status == Models.DownloadStatus.Completed)
            {
                DownloadStatus = "下载完成";
                DownloadHistory historyItem = DownloadHistory.FromTask(resultTask);
                await _historyService.AddHistoryAsync(historyItem);
            }
            else if (resultTask.Status == Models.DownloadStatus.Failed)
            {
                ErrorMessage = resultTask.ErrorMessage;
                DownloadStatus = "下载失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"下载失败: {ex.Message}";
            DownloadStatus = "下载失败";
        }
        finally
        {
            IsDownloading = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
        DownloadStatus = "已取消";
        IsDownloading = false;
    }

    private void Clear()
    {
        VideoUrl = string.Empty;
        VideoInfo = null;
        CurrentTask = null;
        DownloadProgress = 0;
        DownloadStatus = "等待中";
        DownloadSpeed = string.Empty;
        ErrorMessage = string.Empty;
        AvailableQualities.Clear();
        SelectedQuality = "best";
    }

    private void OnDownloadProgress(object? sender, DownloadProgressEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            DownloadProgress = e.Progress;
            DownloadSpeed = e.Speed;
            DownloadStatus = $"下载中... {e.Progress:F1}%";
        });
    }

    private void OnDownloadCompleted(object? sender, DownloadCompletedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsDownloading = false;
            if (e.IsSuccess)
            {
                DownloadStatus = "下载完成";
                DownloadProgress = 100;
            }
            else
            {
                ErrorMessage = e.ErrorMessage ?? "下载失败";
                DownloadStatus = "下载失败";
            }
        });
    }

    private static string SanitizeFileName(string fileName)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}