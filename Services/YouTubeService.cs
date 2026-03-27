using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Services;

public class YouTubeService
{
    private static readonly Lazy<YouTubeService> _instance = new(() => new YouTubeService());
    public static YouTubeService Instance => _instance.Value;

    private readonly DownloadEngineService _engineService;

    public event EventHandler<DownloadProgressEventArgs>? DownloadProgress;
    public event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;

    private YouTubeService()
    {
        _engineService = DownloadEngineService.Instance;
    }

    public async Task<VideoInfo?> GetVideoInfoAsync(string url, CancellationToken cancellationToken = default)
    {
        var ytDlpPath = _engineService.GetEnginePath(EngineType.YtDlp);
        if (!File.Exists(ytDlpPath))
        {
            throw new FileNotFoundException("yt-dlp引擎未安装，请先更新下载引擎。");
        }

        var normalizedUrl = NormalizeYouTubeUrl(url);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                Arguments = $"--dump-json --no-playlist \"{normalizedUrl}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var completed = await Task.Run(() => process.WaitForExit(30000), cancellationToken);

            if (!completed)
            {
                process.Kill();
                throw new Exception("获取视频信息超时，请稍后重试。");
            }

            var output = outputBuilder.ToString().Trim();
            var error = errorBuilder.ToString().Trim();

            if (process.ExitCode != 0)
            {
                throw new Exception($"获取视频信息失败 (ExitCode: {process.ExitCode}): {error}");
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                throw new Exception("获取视频信息失败: yt-dlp 未返回任何数据");
            }

            return ParseVideoInfo(output);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[YouTubeService] GetVideoInfoAsync 错误: {ex.Message}");
            throw;
        }
    }

    private string NormalizeYouTubeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        url = url.Trim();

        if (url.Contains("youtu.be/"))
        {
            var videoIdMatch = Regex.Match(url, @"youtu\.be/([a-zA-Z0-9_-]+)");
            if (videoIdMatch.Success)
            {
                var videoId = videoIdMatch.Groups[1].Value;
                url = $"https://www.youtube.com/watch?v={videoId}";
            }
        }

        url = Regex.Replace(url, @"[?&]list=[^&]*", "");

        return url;
    }

    private VideoInfo? ParseVideoInfo(string jsonOutput)
    {
        try
        {
            jsonOutput = jsonOutput.Trim();

            var firstLine = jsonOutput.Split('\n')[0].Trim();

            using var doc = JsonDocument.Parse(firstLine);
            var root = doc.RootElement;

            var videoInfo = new VideoInfo
            {
                VideoId = GetStringProperty(root, "id"),
                Title = GetStringProperty(root, "title"),
                ThumbnailUrl = GetStringProperty(root, "thumbnail"),
                Duration = GetStringProperty(root, "duration_string"),
                DurationSeconds = GetIntProperty(root, "duration"),
                Uploader = GetStringProperty(root, "uploader"),
                UploadDate = GetStringProperty(root, "upload_date"),
                Description = GetStringProperty(root, "description"),
                ViewCount = GetLongProperty(root, "view_count")
            };

            if (root.TryGetProperty("formats", out var formats))
            {
                foreach (var format in formats.EnumerateArray())
                {
                    var videoFormat = new VideoFormat
                    {
                        FormatId = GetStringProperty(format, "format_id"),
                        Extension = GetStringProperty(format, "ext"),
                        Resolution = GetStringProperty(format, "resolution"),
                        FileSize = GetLongProperty(format, "filesize"),
                        FormatNote = GetStringProperty(format, "format_note")
                    };
                    videoInfo.AvailableFormats.Add(videoFormat);
                }
            }

            var qualities = new HashSet<string>();
            if (root.TryGetProperty("formats", out var fmts))
            {
                foreach (var fmt in fmts.EnumerateArray())
                {
                    var height = GetIntProperty(fmt, "height");
                    if (height > 0)
                    {
                        qualities.Add($"{height}p");
                    }
                }
            }
            videoInfo.AvailableQualities = qualities.OrderByDescending(q => int.Parse(q.TrimEnd('p'))).ToList();

            return videoInfo;
        }
        catch (Exception ex)
        {
            throw new Exception($"解析视频信息失败: {ex.Message}");
        }
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
        {
            return prop.GetString() ?? "";
        }
        return "";
    }

    private static int GetIntProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            return prop.GetInt32();
        }
        return 0;
    }

    private static long GetLongProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            return prop.GetInt64();
        }
        return 0;
    }

    public async Task<DownloadTask> StartDownloadAsync(DownloadTask task, IProgress<DownloadProgressEventArgs>? progress = null, CancellationToken cancellationToken = default)
    {
        var ytDlpPath = _engineService.GetEnginePath(EngineType.YtDlp);
        var ffmpegPath = _engineService.GetEnginePath(EngineType.Ffmpeg);

        if (!File.Exists(ytDlpPath))
            throw new FileNotFoundException("yt-dlp引擎未安装。");

        if (!File.Exists(ffmpegPath))
            throw new FileNotFoundException("ffmpeg引擎未安装。");

        task.Status = DownloadStatus.Downloading;
        task.StartedAt = DateTime.Now;

        System.Diagnostics.Debug.WriteLine($"[YouTubeService] StartDownloadAsync called. Task.SelectedQuality: {task.SelectedQuality}, Task.VideoUrl: {task.VideoUrl}");

        var outputDir = Path.GetDirectoryName(task.OutputPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string qualityArg;
        if (string.IsNullOrEmpty(task.SelectedQuality) || task.SelectedQuality == "best")
        {
            qualityArg = "bestvideo+bestaudio/best";
        }
        else
        {
            var maxHeight = task.SelectedQuality.TrimEnd('p');
            qualityArg = $"bestvideo[height<={maxHeight}]+bestaudio[ext=m4a]";
        }

        var args = $"-f \"{qualityArg}\" --output \"{outputDir}\\%(title)s.%(ext)s\" --merge-output-format mp4 --ffmpeg-location \"{ffmpegPath}\" --no-playlist \"{task.VideoUrl}\"";

        var psi = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = outputDir
        };

        try
        {
            using var process = Process.Start(psi);
            if (process == null)
            {
                task.Status = DownloadStatus.Failed;
                task.ErrorMessage = "无法启动下载进程。";
                return task;
            }

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    ParseProgress(e.Data, task, progress);
            };

            process.BeginOutputReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                task.Status = DownloadStatus.Cancelled;
                process.Kill();
            }
            else if (process.ExitCode == 0)
            {
                task.Status = DownloadStatus.Completed;
                task.CompletedAt = DateTime.Now;
                task.Progress = 100;

                if (File.Exists(task.OutputPath))
                {
                    task.TotalBytes = new FileInfo(task.OutputPath).Length;
                }

                DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(task, true));
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync();
                task.Status = DownloadStatus.Failed;
                task.ErrorMessage = $"下载失败 (Exit code: {process.ExitCode}): {error}";
                DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(task, false, task.ErrorMessage));
            }
        }
        catch (Exception ex)
        {
            task.Status = DownloadStatus.Failed;
            task.ErrorMessage = ex.Message;
            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(task, false, ex.Message));
        }

        return task;
    }

    private void ParseProgress(string data, DownloadTask task, IProgress<DownloadProgressEventArgs>? progress)
    {
        try
        {
            var progressMatch = Regex.Match(data, @"(\d+\.?\d*)%");
            if (progressMatch.Success)
            {
                var percent = double.Parse(progressMatch.Groups[1].Value);
                task.Progress = percent;

                var speedMatch = Regex.Match(data, @"at\s+(.+?)\s+of");
                var etaMatch = Regex.Match(data, @"ETA\s+(.+?)$");

                if (speedMatch.Success)
                    task.DownloadSpeed = speedMatch.Groups[1].Value.Trim();
                if (etaMatch.Success)
                    task.Eta = etaMatch.Groups[1].Value.Trim();

                var eventArgs = new DownloadProgressEventArgs(task.Id, percent, task.DownloadedBytes, task.TotalBytes, task.DownloadSpeed, task.Eta);
                progress?.Report(eventArgs);
                DownloadProgress?.Invoke(this, eventArgs);
            }
        }
        catch
        {
        }
    }

    public void CancelDownload(string taskId)
    {
    }
}

public class DownloadProgressEventArgs : EventArgs
{
    public string TaskId { get; }
    public double Progress { get; }
    public long DownloadedBytes { get; }
    public long TotalBytes { get; }
    public string Speed { get; }
    public string Eta { get; }

    public DownloadProgressEventArgs(string taskId, double progress, long downloaded, long total, string speed, string eta)
    {
        TaskId = taskId;
        Progress = progress;
        DownloadedBytes = downloaded;
        TotalBytes = total;
        Speed = speed;
        Eta = eta;
    }
}

public class DownloadCompletedEventArgs : EventArgs
{
    public DownloadTask Task { get; }
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    public DownloadCompletedEventArgs(DownloadTask task, bool isSuccess, string? errorMessage = null)
    {
        Task = task;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}