using System.Windows.Controls;
using System.Windows.Input;
using AutoInsert.UI.ViewModels;

namespace AutoInsert.UI.Pages;

public partial class DebugPage : Page
{
    public DebugPage()
    {
        InitializeComponent();
        Loaded += DebugPage_Loaded;
        PreviewKeyDown += DebugPage_PreviewKeyDown;
        Focusable = true;
    }

    private async void DebugPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DebugViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
        
        // Set focus to enable keyboard input
        Keyboard.Focus(this);
    }

    private void DebugPage_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            // Stop polling before navigating away
            if (DataContext is DebugViewModel viewModel)
            {
                viewModel.StopAllPolling();
            }
            
            // Navigate back to configuration page
            NavigationService?.Navigate(new ConfigurationPage());
            
            e.Handled = true;
        }
    }
}