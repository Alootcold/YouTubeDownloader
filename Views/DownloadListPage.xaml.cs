using Microsoft.UI.Xaml.Controls;
using YouTubeDownloader.Models;
using YouTubeDownloader.ViewModels;

namespace YouTubeDownloader.Views;

public sealed partial class DownloadListPage : Page
{
    public DownloadListPage()
    {
        this.InitializeComponent();
        this.DataContext = new DownloadListViewModel();
    }

    private void PauseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DownloadTask task)
        {
            if (task.Status == DownloadStatus.Downloading)
            {
                task.Status = DownloadStatus.Paused;
            }
            else if (task.Status == DownloadStatus.Paused)
            {
                task.Status = DownloadStatus.Downloading;
            }
        }
    }

    private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DownloadTask task)
        {
            task.Status = DownloadStatus.Cancelled;
        }
    }
}