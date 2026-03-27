using System.Text.Json.Serialization;

namespace YouTubeDownloader.Models;

public enum EngineType
{
    YtDlp,
    Ffmpeg,
    Deno
}

public class EngineInfo
{
    [JsonPropertyName("type")]
    public EngineType Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("lastChecked")]
    public DateTime LastChecked { get; set; }

    [JsonPropertyName("isInstalled")]
    public bool IsInstalled { get; set; }

    [JsonPropertyName("localPath")]
    public string LocalPath { get; set; } = string.Empty;

    public static EngineInfo GetYtDlpInfo()
    {
        return new EngineInfo
        {
            Type = EngineType.YtDlp,
            Name = "yt-dlp",
            FileName = "yt-dlp.exe",
            DownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
        };
    }

    public static EngineInfo GetFfmpegInfo()
    {
        return new EngineInfo
        {
            Type = EngineType.Ffmpeg,
            Name = "ffmpeg",
            FileName = "ffmpeg.exe",
            DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
        };
    }

    public static EngineInfo GetDenoInfo()
    {
        return new EngineInfo
        {
            Type = EngineType.Deno,
            Name = "Deno",
            FileName = "deno.exe",
            DownloadUrl = "https://github.com/denoland/deno/releases/latest/download/deno-x86_64-pc-windows-msvc.zip"
        };
    }
}

public class AppSettings
{
    [JsonPropertyName("downloadPath")]
    public string DownloadPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "YouTubeDownloader");

    [JsonPropertyName("defaultQuality")]
    public string DefaultQuality { get; set; } = "best";

    [JsonPropertyName("defaultFormat")]
    public string DefaultFormat { get; set; } = "mp4";

    [JsonPropertyName("maxConcurrentDownloads")]
    public int MaxConcurrentDownloads { get; set; } = 3;

    [JsonPropertyName("ytDlpVersion")]
    public string YtDlpVersion { get; set; } = string.Empty;

    [JsonPropertyName("ffmpegVersion")]
    public string FfmpegVersion { get; set; } = string.Empty;

    [JsonPropertyName("denoVersion")]
    public string DenoVersion { get; set; } = string.Empty;

    [JsonPropertyName("lastEngineCheck")]
    public DateTime LastEngineCheck { get; set; }
}