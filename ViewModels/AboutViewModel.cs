namespace YouTubeDownloader.ViewModels;

public class AboutViewModel : ViewModelBase
{
    public AboutViewModel()
    {
        AppName = "YouTube视频下载器 换新版";
        Version = "2.0.0";
        Description = "一款简洁高效的YouTube视频下载器，支持多种格式和画质选择。";
        Copyright = "© 2026 YouTubeDownloader";
        License = "MIT License";
    }

    public string AppName { get; }
    public string Version { get; }
    public string Description { get; }
    public string Copyright { get; }
    public string License { get; }

    public string[] Features { get; } = new[]
    {
        "支持YouTube视频下载",
        "多种画质和格式选择",
        "内置ffmpeg视频处理",
        "实时下载进度显示",
        "下载历史记录管理",
        "简洁直观的用户界面"
    };
}