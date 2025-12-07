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

    private async void ReconnectButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DebugViewModel)DataContext;
        await viewModel.ReconnectAsync();
    }

    private async void EnableFreedriveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DebugViewModel)DataContext;
        await viewModel.EnableFreedriveAsync();
    }

    private async void DisableFreedriveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DebugViewModel)DataContext;
        await viewModel.DisableFreedriveAsync();
    }

    private async void SendScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DebugViewModel)DataContext;
        await viewModel.SendCustomScriptAsync();
    }

    private void SaveWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DebugViewModel)DataContext;
        viewModel.SaveCurrentWaypoint();
    }

    private void DeleteWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DebugViewModel)DataContext;
        viewModel.DeleteSelectedWaypoint();
    }

    private async void MoveToWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DebugViewModel)DataContext;
        await viewModel.MoveToSelectedWaypointAsync();
    }
}