using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Data;

namespace AutoInsert.UI.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ConfigurationController _configController;
    
    public SettingsViewModel()
    {
        _configController = new ConfigurationController(new StorageService());
        LoadConfigurationCommand();
    }

    private string _robotIpAddress = "";
    public string RobotIpAddress
    {
        get => _robotIpAddress;
        set
        {
            _robotIpAddress = value;
            OnPropertyChanged();
        }
    }

    private string _selectedSerialPort = "";
    public string SelectedSerialPort
    {
        get => _selectedSerialPort;
        set
        {
            _selectedSerialPort = value;
            OnPropertyChanged();
        }
    }

    private int _serialBaudRate = 115200;
    public int SerialBaudRate
    {
        get => _serialBaudRate;
        set
        {
            _serialBaudRate = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<string> _availableSerialPorts = new();
    public ObservableCollection<string> AvailableSerialPorts
    {
        get => _availableSerialPorts;
        set
        {
            _availableSerialPorts = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<int> _availableBaudRates = new()
    {
        9600, 19200, 38400, 57600, 115200, 230400
    };
    public ObservableCollection<int> AvailableBaudRates
    {
        get => _availableBaudRates;
        set
        {
            _availableBaudRates = value;
            OnPropertyChanged();
        }
    }

    private bool _isCalibrated;
    public bool IsCalibrated
    {
        get => _isCalibrated;
        set
        {
            _isCalibrated = value;
            OnPropertyChanged();
        }
    }

    private DateTime? _lastCalibrationTime;
    public DateTime? LastCalibrationTime
    {
        get => _lastCalibrationTime;
        set
        {
            _lastCalibrationTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CalibrationTimeText));
        }
    }

    public string CalibrationTimeText => LastCalibrationTime.HasValue 
        ? LastCalibrationTime.Value.ToString("yyyy-MM-dd HH:mm:ss") 
        : "Never";

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadConfigurationAsync()
    {
        try
        {
            await _configController.LoadConfigurationAsync();
            RobotIpAddress = _configController.GetRobotIpAddress() ?? "";
            SelectedSerialPort = _configController.GetSerialPort() ?? "";
            SerialBaudRate = _configController.GetSerialBaudRate();
            IsCalibrated = _configController.HasCalibration();
            LastCalibrationTime = _configController.GetLastCalibrationTime();
            StatusMessage = "Configuration loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading: {ex.Message}";
        }
    }

    private async void LoadConfigurationCommand()
    {
        await LoadConfigurationAsync();
        LoadAvailableSerialPorts();
    }

    public async Task SaveRobotIpAddressAsync()
    {
        try
        {
            bool success = await _configController.SetRobotIpAddressAsync(RobotIpAddress);
            StatusMessage = success ? "Robot IP saved" : "Failed to save";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public async Task SaveSerialPortAsync()
    {
        try
        {
            bool success = await _configController.SetSerialPortAsync(SelectedSerialPort, SerialBaudRate);
            StatusMessage = success ? "Serial port saved" : "Failed to save";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public void LoadAvailableSerialPorts()
    {
        try
        {
            var ports = ConfigurationController.GetAvailableSerialPorts();
            AvailableSerialPorts.Clear();
            foreach (var port in ports)
            {
                AvailableSerialPorts.Add(port);
            }
            StatusMessage = $"Found {ports.Length} serial port(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading ports: {ex.Message}";
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        try
        {
            bool success = await _configController.ResetToDefaultsAsync();
            if (success)
            {
                await LoadConfigurationAsync();
                StatusMessage = "Reset to defaults";
            }
            else
            {
                StatusMessage = "Failed to reset";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
