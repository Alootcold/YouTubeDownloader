using Microsoft.UI.Xaml.Controls;
using YouTubeDownloader.ViewModels;

namespace YouTubeDownloader.Views;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
        this.DataContext = new AboutViewModel();
    }
}