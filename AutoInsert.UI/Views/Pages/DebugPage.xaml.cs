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
        Unloaded += DebugPage_Unloaded;
    }

    private void DebugPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.Disconnect();
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

    private void AddLocalWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.AddLocalWaypoint();
    }

    private void DeleteLocalWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.DeleteLocalWaypoint();
    }

    private async void MoveToLocalWaypointButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.MoveToLocalWaypointAsync();
    }

    private async void MoveToCoordinatesButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.MoveToCoordinatesAsync();
    }

    private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        // Allow digits, decimal point, and minus sign
        var textBox = sender as TextBox;
        if (textBox == null) return;
        
        string text = textBox.Text;
        string newText = text.Insert(textBox.SelectionStart, e.Text);
        
        // Check if the resulting text is a valid number
        e.Handled = !IsValidNumericInput(newText);
    }

    private bool IsValidNumericInput(string text)
    {
        // Allow empty, minus sign, decimal point, or valid number
        if (string.IsNullOrEmpty(text) || text == "-" || text == "." || text == "-.")
            return true;
        
        return double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
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
}