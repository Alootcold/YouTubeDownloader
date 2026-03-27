using Microsoft.UI.Xaml.Controls;
using YouTubeDownloader.ViewModels;

namespace YouTubeDownloader.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
        this.DataContext = new HomeViewModel();
    }
}