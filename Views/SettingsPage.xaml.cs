using Microsoft.UI.Xaml.Controls;
using YouTubeDownloader.ViewModels;

namespace YouTubeDownloader.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.DataContext = new SettingsViewModel();
    }
}