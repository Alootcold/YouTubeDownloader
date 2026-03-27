using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using YouTubeDownloader.Models;
using YouTubeDownloader.Services;

namespace YouTubeDownloader.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly DownloadEngineService _engineService;
    private readonly SettingsService _settingsService;
    private readonly DispatcherQueue _dispatcherQueue;

    private string _downloadPath = string.Empty;
    private string _ytDlpVersion = string.Empty;
    private string _ffmpegVersion = string.Empty;
    private string _denoVersion = string.Empty;
    private bool _isYtDlpInstalled;
    private bool _isFfmpegInstalled;
    private bool _isDenoInstalled;
    private bool _isUpdatingYtDlp;
    private bool _isUpdatingFfmpeg;
    private bool _isUpdatingDeno;
    private int _ytDlpProgress;
    private int _ffmpegProgress;
    private int _denoProgress;
    private string _statusMessage = string.Empty;

    public SettingsViewModel()
    {
        _engineService = DownloadEngineService.Instance;
        _settingsService = SettingsService.Instance;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _engineService.EngineUpdateStarted += OnEngineUpdateStarted;
        _engineService.EngineUpdateCompleted += OnEngineUpdateCompleted;
        _engineService.EngineUpdateFailed += OnEngineUpdateFailed;

        BrowseCommand = new RelayCommand(Browse);
        UpdateYtDlpCommand = new AsyncRelayCommand(() => UpdateEngineAsync(EngineType.YtDlp));
        UpdateFfmpegCommand = new AsyncRelayCommand(() => UpdateEngineAsync(EngineType.Ffmpeg));
        UpdateDenoCommand = new AsyncRelayCommand(() => UpdateEngineAsync(EngineType.Deno));
        UpdateAllCommand = new AsyncRelayCommand(UpdateAllEnginesAsync);

        LoadSettings();
    }

    public string DownloadPath
    {
        get => _downloadPath;
        set
        {
            if (SetProperty(ref _downloadPath, value))
            {
                _ = _settingsService.SetDownloadPathAsync(value);
            }
        }
    }

    public string YtDlpVersion
    {
        get => _ytDlpVersion;
        set => SetProperty(ref _ytDlpVersion, value);
    }

    public string FfmpegVersion
    {
        get => _ffmpegVersion;
        set => SetProperty(ref _ffmpegVersion, value);
    }

    public string DenoVersion
    {
        get => _denoVersion;
        set => SetProperty(ref _denoVersion, value);
    }

    public bool IsYtDlpInstalled
    {
        get => _isYtDlpInstalled;
        set => SetProperty(ref _isYtDlpInstalled, value);
    }

    public bool IsFfmpegInstalled
    {
        get => _isFfmpegInstalled;
        set => SetProperty(ref _isFfmpegInstalled, value);
    }

    public bool IsDenoInstalled
    {
        get => _isDenoInstalled;
        set => SetProperty(ref _isDenoInstalled, value);
    }

    public bool IsUpdatingYtDlp
    {
        get => _isUpdatingYtDlp;
        set => SetProperty(ref _isUpdatingYtDlp, value);
    }

    public bool IsUpdatingFfmpeg
    {
        get => _isUpdatingFfmpeg;
        set => SetProperty(ref _isUpdatingFfmpeg, value);
    }

    public bool IsUpdatingDeno
    {
        get => _isUpdatingDeno;
        set => SetProperty(ref _isUpdatingDeno, value);
    }

    public int YtDlpProgress
    {
        get => _ytDlpProgress;
        set => SetProperty(ref _ytDlpProgress, value);
    }

    public int FfmpegProgress
    {
        get => _ffmpegProgress;
        set => SetProperty(ref _ffmpegProgress, value);
    }

    public int DenoProgress
    {
        get => _denoProgress;
        set => SetProperty(ref _denoProgress, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand BrowseCommand { get; }
    public AsyncRelayCommand UpdateYtDlpCommand { get; }
    public AsyncRelayCommand UpdateFfmpegCommand { get; }
    public AsyncRelayCommand UpdateDenoCommand { get; }
    public AsyncRelayCommand UpdateAllCommand { get; }

    private async void LoadSettings()
    {
        var settings = _settingsService.GetSettings();
        _dispatcherQueue.TryEnqueue(() =>
        {
            DownloadPath = settings.DownloadPath;
        });

        _dispatcherQueue.TryEnqueue(() =>
        {
            IsYtDlpInstalled = _engineService.IsEngineInstalled(EngineType.YtDlp);
            IsFfmpegInstalled = _engineService.IsEngineInstalled(EngineType.Ffmpeg);
            IsDenoInstalled = _engineService.IsEngineInstalled(EngineType.Deno);
        });

        var ytDlpVer = await _engineService.GetEngineVersionAsync(EngineType.YtDlp);
        _dispatcherQueue.TryEnqueue(() =>
        {
            YtDlpVersion = IsYtDlpInstalled ? (ytDlpVer ?? "未知版本") : "未安装";
        });

        var ffmpegVer = await _engineService.GetEngineVersionAsync(EngineType.Ffmpeg);
        _dispatcherQueue.TryEnqueue(() =>
        {
            FfmpegVersion = IsFfmpegInstalled ? (ffmpegVer ?? "未知版本") : "未安装";
        });

        var denoVer = await _engineService.GetEngineVersionAsync(EngineType.Deno);
        _dispatcherQueue.TryEnqueue(() =>
        {
            DenoVersion = IsDenoInstalled ? (denoVer ?? "未知版本") : "未安装";
        });
    }

    private async void Browse()
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
        picker.FileTypeFilter.Add("*");

        var mainWindow = MainWindow.Instance;
        if (mainWindow != null)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        }

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            DownloadPath = folder.Path;
        }
    }

    private async Task UpdateEngineAsync(EngineType type)
    {
        var progress = new Progress<int>(p =>
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                switch (type)
                {
                    case EngineType.YtDlp:
                        YtDlpProgress = p;
                        IsUpdatingYtDlp = true;
                        break;
                    case EngineType.Ffmpeg:
                        FfmpegProgress = p;
                        IsUpdatingFfmpeg = true;
                        break;
                    case EngineType.Deno:
                        DenoProgress = p;
                        IsUpdatingDeno = true;
                        break;
                }
            });
        });

        var success = await _engineService.UpdateEngineAsync(type, progress);

        _dispatcherQueue.TryEnqueue(() =>
        {
            switch (type)
            {
                case EngineType.YtDlp:
                    IsUpdatingYtDlp = false;
                    YtDlpProgress = 0;
                    break;
                case EngineType.Ffmpeg:
                    IsUpdatingFfmpeg = false;
                    FfmpegProgress = 0;
                    break;
                case EngineType.Deno:
                    IsUpdatingDeno = false;
                    DenoProgress = 0;
                    break;
            }
        });
    }

    private async Task UpdateAllEnginesAsync()
    {
        await UpdateEngineAsync(EngineType.YtDlp);
        await UpdateEngineAsync(EngineType.Ffmpeg);
        await UpdateEngineAsync(EngineType.Deno);
        LoadSettings();
    }

    private void OnEngineUpdateStarted(object? sender, EngineUpdateEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            StatusMessage = $"正在更新{e.EngineName}...";
        });
    }

    private void OnEngineUpdateCompleted(object? sender, EngineUpdateEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            StatusMessage = $"{e.EngineName} 已更新到版本: {e.Version}";
            LoadSettings();

            ShowUpdateNotification(e.EngineName);
        });
    }

    private void OnEngineUpdateFailed(object? sender, EngineUpdateEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            StatusMessage = $"{e.EngineName} 更新失败: {e.ErrorMessage}";
        });
    }

    private async void ShowUpdateNotification(string engineName)
    {
        var dialog = new ContentDialog
        {
            Title = "更新完成",
            Content = $"{engineName} 已成功更新！请重启应用程序以使用新版本。",
            CloseButtonText = "确定"
        };

        var mainWindow = MainWindow.Instance;
        if (mainWindow != null)
        {
            dialog.XamlRoot = mainWindow.Content.XamlRoot;
        }

        await dialog.ShowAsync();
    }
}