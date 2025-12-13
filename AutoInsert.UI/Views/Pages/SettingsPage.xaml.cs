using System.Windows;
using System.Windows.Controls;
using AutoInsert.UI.ViewModels;
using Wpf.Ui.Controls;

namespace AutoInsert.UI.Pages;

public partial class SettingsPage : Page
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;
    }

    private async void SaveRobotIpButton_Click(object sender, RoutedEventArgs e)
    {
        var result = await _viewModel.SaveRobotIpAddressAsync();
        ShowSnackbar(result.Success ? "Saved & Connected" : "Error", 
            result.Message, 
            result.Success ? ControlAppearance.Success : ControlAppearance.Danger);
    }

    private async void SaveSerialPortButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveSerialPortAsync();
        ShowSnackbar("Saved", "Serial port settings saved successfully", ControlAppearance.Success);
    }

    private void RefreshPortsButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadAvailableSerialPorts();
    }

    private async void CaptureOriginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _viewModel.CaptureOriginPointAsync();
            ShowSnackbar(result.Success ? "Captured" : "Error", result.Message, 
                result.Success ? ControlAppearance.Secondary : ControlAppearance.Danger);
        }
        catch (Exception ex)
        {
            ShowSnackbar("Error", $"Failed to capture: {ex.Message}", ControlAppearance.Danger);
        }
    }

    private async void CaptureXAxisButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _viewModel.CaptureXAxisPointAsync();
            ShowSnackbar(result.Success ? "Captured" : "Error", result.Message, 
                result.Success ? ControlAppearance.Secondary : ControlAppearance.Danger);
        }
        catch (Exception ex)
        {
            ShowSnackbar("Error", $"Failed to capture: {ex.Message}", ControlAppearance.Danger);
        }
    }

    private async void CaptureYAxisButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _viewModel.CaptureYAxisPointAsync();
            ShowSnackbar(result.Success ? "Captured" : "Error", result.Message, 
                result.Success ? ControlAppearance.Secondary : ControlAppearance.Danger);
        }
        catch (Exception ex)
        {
            ShowSnackbar("Error", $"Failed to capture: {ex.Message}", ControlAppearance.Danger);
        }
    }

    private async void PerformCalibrationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _viewModel.PerformCalibrationAsync();
            ShowSnackbar(result.Success ? "Success" : "Error", result.Message, 
                result.Success ? ControlAppearance.Success : ControlAppearance.Danger);
        }
        catch (Exception ex)
        {
            ShowSnackbar("Error", $"Calibration failed: {ex.Message}", ControlAppearance.Danger);
        }
    }

    private async void RetryConnectionButton_Click(object sender, RoutedEventArgs e)
    {
        var result = await _viewModel.ConnectToRobotAsync();
        ShowSnackbar(result.Success ? "Connected" : "Connection Failed", result.Message, 
            result.Success ? ControlAppearance.Success : ControlAppearance.Danger);
    }

    private void ShowSnackbar(string title, string message, ControlAppearance appearance)
    {
        var snackbar = new Snackbar(SnackbarPresenter)
        {
            Title = title,
            Content = message,
            Appearance = appearance,
            Timeout = TimeSpan.FromSeconds(3)
        };
        snackbar.Show();
    }
}
