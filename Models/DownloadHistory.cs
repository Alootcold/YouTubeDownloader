using System.Text.Json.Serialization;

namespace YouTubeDownloader.Models;

public class DownloadHistory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("videoUrl")]
    public string VideoUrl { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public string Duration { get; set; } = string.Empty;

    [JsonPropertyName("uploader")]
    public string Uploader { get; set; } = string.Empty;

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonIgnore]
    public bool HasFilePath => !string.IsNullOrEmpty(FilePath);

    [JsonIgnore]
    public string DownloadedAtText => DownloadedAt.ToString("yyyy-MM-dd HH:mm");

    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("quality")]
    public string Quality { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public DownloadStatus Status { get; set; }

    [JsonPropertyName("downloadedAt")]
    public DateTime DownloadedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    public static DownloadHistory FromTask(DownloadTask task)
    {
        return new DownloadHistory
        {
            Id = task.Id,
            VideoUrl = task.VideoUrl,
            Title = task.VideoInfo?.Title ?? "Unknown",
            ThumbnailUrl = task.VideoInfo?.ThumbnailUrl ?? "",
            Duration = task.VideoInfo?.Duration ?? "",
            Uploader = task.VideoInfo?.Uploader ?? "",
            FilePath = task.OutputPath,
            FileSize = task.TotalBytes,
            Format = task.SelectedFormat,
            Quality = task.SelectedQuality,
            Status = task.Status,
            DownloadedAt = DateTime.Now,
            ErrorMessage = task.ErrorMessage
        };
    }
}