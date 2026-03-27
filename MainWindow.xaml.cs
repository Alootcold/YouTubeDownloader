using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace YouTubeDownloader;

public sealed partial class MainWindow : Window
{
    private static MainWindow? _instance;

    public static MainWindow? Instance => _instance;

    public MainWindow()
    {
        _instance = this;
        InitializeComponent();
        ActivateWindow();
    }

    private void ActivateWindow()
    {
        if (NavView.MenuItems.Count > 0)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }
        ContentFrame.Navigate(typeof(Views.HomePage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            Type? pageType = tag switch
            {
                "Home" => typeof(Views.HomePage),
                "DownloadList" => typeof(Views.DownloadListPage),
                "History" => typeof(Views.HistoryPage),
                "Settings" => typeof(Views.SettingsPage),
                "About" => typeof(Views.AboutPage),
                _ => typeof(Views.HomePage)
            };

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}