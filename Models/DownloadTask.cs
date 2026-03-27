using System.Text.Json.Serialization;

namespace YouTubeDownloader.Models;

public enum DownloadStatus
{
    Waiting,
    Downloading,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public class DownloadTask
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("videoUrl")]
    public string VideoUrl { get; set; } = string.Empty;

    [JsonPropertyName("videoInfo")]
    public VideoInfo? VideoInfo { get; set; }

    [JsonPropertyName("status")]
    public DownloadStatus Status { get; set; } = DownloadStatus.Waiting;

    [JsonPropertyName("progress")]
    public double Progress { get; set; }

    [JsonIgnore]
    public string ProgressText => $"{Progress:F1}%";

    [JsonPropertyName("downloadedBytes")]
    public long DownloadedBytes { get; set; }

    [JsonPropertyName("totalBytes")]
    public long TotalBytes { get; set; }

    [JsonIgnore]
    public string DownloadSpeed { get; set; } = string.Empty;

    [JsonIgnore]
    public string Eta { get; set; } = string.Empty;

    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = string.Empty;

    [JsonPropertyName("selectedFormat")]
    public string SelectedFormat { get; set; } = string.Empty;

    [JsonPropertyName("selectedQuality")]
    public string SelectedQuality { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;
}