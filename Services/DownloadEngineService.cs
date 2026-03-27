using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Services;

public class DownloadEngineService
{
    private static readonly Lazy<DownloadEngineService> _instance = new(() => new DownloadEngineService());
    public static DownloadEngineService Instance => _instance.Value;

    private readonly HttpClient _httpClient;
    private readonly string _enginesFolder;
    private readonly string _settingsFile;

    public event EventHandler<EngineUpdateEventArgs>? EngineUpdateStarted;
    public event EventHandler<EngineUpdateEventArgs>? EngineUpdateCompleted;
    public event EventHandler<EngineUpdateEventArgs>? EngineUpdateFailed;

    private DownloadEngineService()
    {
        _httpClient = new HttpClient();
        try
        {
            _enginesFolder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Engines");
        }
        catch
        {
            _enginesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Engines");
        }
        _settingsFile = Path.Combine(_enginesFolder, "settings.json");
        EnsureEnginesFolder();
    }

    private void EnsureEnginesFolder()
    {
        if (!Directory.Exists(_enginesFolder))
        {
            Directory.CreateDirectory(_enginesFolder);
        }
    }

    public string GetEnginePath(EngineType type)
    {
        var fileName = type switch
        {
            EngineType.YtDlp => "yt-dlp.exe",
            EngineType.Ffmpeg => "ffmpeg.exe",
            EngineType.Deno => "deno.exe",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        return Path.Combine(_enginesFolder, fileName);
    }

    public bool IsEngineInstalled(EngineType type)
    {
        var path = GetEnginePath(type);
        return File.Exists(path);
    }

    public async Task<string?> GetEngineVersionAsync(EngineType type)
    {
        var path = GetEnginePath(type);
        if (!File.Exists(path))
            return null;

        try
        {
            var args = type switch
            {
                EngineType.YtDlp => "--version",
                EngineType.Ffmpeg => "-version",
                EngineType.Deno => "--version",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            var psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                return null;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Trim().Split('\n')[0].Trim();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateEngineAsync(EngineType type, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        var engineInfo = type switch
        {
            EngineType.YtDlp => EngineInfo.GetYtDlpInfo(),
            EngineType.Ffmpeg => EngineInfo.GetFfmpegInfo(),
            EngineType.Deno => EngineInfo.GetDenoInfo(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        EngineUpdateStarted?.Invoke(this, new EngineUpdateEventArgs(type, engineInfo.Name));

        try
        {
            var downloadPath = Path.Combine(_enginesFolder, Path.GetFileName(engineInfo.DownloadUrl));

            await DownloadFileAsync(engineInfo.DownloadUrl, downloadPath, progress, cancellationToken);

            if (type == EngineType.Ffmpeg)
            {
                await ExtractFfmpegAsync(downloadPath, cancellationToken);
                File.Delete(downloadPath);
            }
            else if (type == EngineType.Deno)
            {
                await ExtractDenoAsync(downloadPath, cancellationToken);
                File.Delete(downloadPath);
            }

            var version = await GetEngineVersionAsync(type);
            await SaveEngineVersionAsync(type, version ?? "unknown");

            EngineUpdateCompleted?.Invoke(this, new EngineUpdateEventArgs(type, engineInfo.Name, version));
            return true;
        }
        catch (Exception ex)
        {
            EngineUpdateFailed?.Invoke(this, new EngineUpdateEventArgs(type, engineInfo.Name, errorMessage: ex.Message));
            return false;
        }
    }

    private async Task DownloadFileAsync(string url, string destination, IProgress<int>? progress, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var buffer = new byte[8192];
        var totalRead = 0L;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await contentStream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                break;

            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            if (totalBytes > 0)
            {
                var percent = (int)((totalRead * 100) / totalBytes);
                progress?.Report(percent);
            }
        }
    }

    private Task ExtractFfmpegAsync(string zipPath, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var extractPath = Path.Combine(_enginesFolder, "ffmpeg-temp");
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

            var exeFiles = Directory.GetFiles(extractPath, "ffmpeg.exe", SearchOption.AllDirectories);
            if (exeFiles.Length > 0)
            {
                var destPath = Path.Combine(_enginesFolder, "ffmpeg.exe");
                File.Copy(exeFiles[0], destPath, true);
            }

            Directory.Delete(extractPath, true);
        }, cancellationToken);
    }

    private Task ExtractDenoAsync(string zipPath, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var extractPath = Path.Combine(_enginesFolder, "deno-temp");
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

            var exeFiles = Directory.GetFiles(extractPath, "deno.exe", SearchOption.AllDirectories);
            if (exeFiles.Length > 0)
            {
                var destPath = Path.Combine(_enginesFolder, "deno.exe");
                File.Copy(exeFiles[0], destPath, true);
            }

            Directory.Delete(extractPath, true);
        }, cancellationToken);
    }

    private async Task SaveEngineVersionAsync(EngineType type, string version)
    {
        var settings = await LoadSettingsAsync();
        switch (type)
        {
            case EngineType.YtDlp:
                settings.YtDlpVersion = version;
                break;
            case EngineType.Ffmpeg:
                settings.FfmpegVersion = version;
                break;
            case EngineType.Deno:
                settings.DenoVersion = version;
                break;
        }
        await SaveSettingsAsync(settings);
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        if (!File.Exists(_settingsFile))
            return new AppSettings();

        var json = await File.ReadAllTextAsync(_settingsFile);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFile, json);
    }

    public async Task EnsureEnginesInstalledAsync()
    {
        if (!IsEngineInstalled(EngineType.YtDlp))
        {
            await UpdateEngineAsync(EngineType.YtDlp);
        }
        if (!IsEngineInstalled(EngineType.Ffmpeg))
        {
            await UpdateEngineAsync(EngineType.Ffmpeg);
        }
        if (!IsEngineInstalled(EngineType.Deno))
        {
            await UpdateEngineAsync(EngineType.Deno);
        }
    }
}

public class EngineUpdateEventArgs : EventArgs
{
    public EngineType EngineType { get; }
    public string EngineName { get; }
    public string? Version { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

    public EngineUpdateEventArgs(EngineType type, string name, string? version = null, string? errorMessage = null)
    {
        EngineType = type;
        EngineName = name;
        Version = version;
        ErrorMessage = errorMessage;
    }
}