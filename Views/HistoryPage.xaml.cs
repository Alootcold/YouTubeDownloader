using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using YouTubeDownloader.Models;
using YouTubeDownloader.ViewModels;

namespace YouTubeDownloader.Views;

public sealed partial class HistoryPage : Page
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage()
    {
        this.InitializeComponent();
        _viewModel = new HistoryViewModel();
        this.DataContext = _viewModel;
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DownloadHistory history)
        {
            if (!string.IsNullOrEmpty(history.FilePath) && System.IO.File.Exists(history.FilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = history.FilePath,
                    UseShellExecute = true
                });
            }
        }
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DownloadHistory history)
        {
            var folder = System.IO.Path.GetDirectoryName(history.FilePath);
            if (!string.IsNullOrEmpty(folder) && System.IO.Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{history.FilePath}\"",
                    UseShellExecute = true
                });
            }
        }
    }

    private async void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DownloadHistory history)
        {
            await _viewModel.DeleteRecordCommand.ExecuteAsync(history);
        }
    }
}