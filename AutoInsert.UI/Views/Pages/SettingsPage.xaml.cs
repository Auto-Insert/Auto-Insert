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
        await _viewModel.SaveRobotIpAddressAsync();
        ShowSnackbar("Saved", "Robot IP address saved successfully");
    }

    private async void SaveSerialPortButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveSerialPortAsync();
        ShowSnackbar("Saved", "Serial port settings saved successfully");
    }

    private void RefreshPortsButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadAvailableSerialPorts();
    }

    private void ShowSnackbar(string title, string message)
    {
        var snackbar = new Snackbar(SnackbarPresenter)
        {
            Title = title,
            Content = message,
            Appearance = ControlAppearance.Success,
            Timeout = TimeSpan.FromSeconds(3)
        };
        snackbar.Show();
    }
}
