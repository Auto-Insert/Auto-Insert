using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AutoInsert.UI.Pages;

public partial class CompletionPage : Page
{
    private ProcessResult _result;

    public CompletionPage(ProcessResult result)
    {
        InitializeComponent();
        _result = result;
    }

    private void StopApplication(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void RestartProcess(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ConfigurationPage());
    }
}