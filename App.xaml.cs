using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using YouTubeDownloader.Services;

namespace YouTubeDownloader;

public partial class App : Application
{
    public static DownloadEngineService EngineService { get; } = DownloadEngineService.Instance;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        window.Activate();
    }
}