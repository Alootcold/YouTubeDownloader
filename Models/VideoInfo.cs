using System.Text.Json.Serialization;

namespace YouTubeDownloader.Models;

public class VideoInfo
{
    [JsonPropertyName("videoId")]
    public string VideoId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public string Duration { get; set; } = string.Empty;

    [JsonPropertyName("durationSeconds")]
    public int DurationSeconds { get; set; }

    [JsonPropertyName("uploader")]
    public string Uploader { get; set; } = string.Empty;

    [JsonPropertyName("uploadDate")]
    public string UploadDate { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("viewCount")]
    public long ViewCount { get; set; }

    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    [JsonPropertyName("availableFormats")]
    public List<VideoFormat> AvailableFormats { get; set; } = new();

    [JsonPropertyName("availableQualities")]
    public List<string> AvailableQualities { get; set; } = new();
}

public class VideoFormat
{
    [JsonPropertyName("formatId")]
    public string FormatId { get; set; } = string.Empty;

    [JsonPropertyName("ext")]
    public string Extension { get; set; } = string.Empty;

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = string.Empty;

    [JsonPropertyName("filesize")]
    public long FileSize { get; set; }

    [JsonPropertyName("formatNote")]
    public string FormatNote { get; set; } = string.Empty;
}