using System.Windows.Controls;
using AutoInsert.UI.ViewModels;

namespace AutoInsert.UI.Pages;

public partial class DebugPage : Page
{
    private readonly DebugViewModel _viewModel;

    public DebugPage()
    {
        InitializeComponent();
        _viewModel = new DebugViewModel();
        DataContext = _viewModel;
    }

    private async void ReconnectButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.ReconnectAsync();
    }

    private async void EnableFreedriveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.EnableFreedriveAsync();
    }

    private async void DisableFreedriveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.DisableFreedriveAsync();
    }

    private async void SendScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.SendCustomScriptAsync();
    }

    private void SaveWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.SaveCurrentWaypoint();
    }

    private void DeleteWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.DeleteSelectedWaypoint();
    }

    private async void MoveToWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.MoveToSelectedWaypointAsync();
    }

    private async void SetScrewdriverExtensionButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.SetScrewdriverExtensionAsync();
    }

    private async void MoveServoButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.MoveServoMotorAsync();
    }

    private async void MoveStepperButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.MoveStepperMotorAsync();
    }

    private async void MoveSolenoidButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.MoveSolenoidActuatorAsync();
    }

    private async void RefreshPortsButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.LoadAvailableSerialPortsAsync();
    }
}