using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Data;
using AutoInsert.Core.Services.Control;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ConfigurationController _configController;
    private URController? _urController;
    private CalibrationController? _calibrationController;
    
    private Waypoint? _originPoint;
    private Waypoint? _xAxisPoint;
    private Waypoint? _yAxisPoint;
    
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

    private string _originPointStatus = "Not captured";
    public string OriginPointStatus
    {
        get => _originPointStatus;
        set
        {
            _originPointStatus = value;
            OnPropertyChanged();
        }
    }

    private string _xAxisPointStatus = "Not captured";
    public string XAxisPointStatus
    {
        get => _xAxisPointStatus;
        set
        {
            _xAxisPointStatus = value;
            OnPropertyChanged();
        }
    }

    private string _yAxisPointStatus = "Not captured";
    public string YAxisPointStatus
    {
        get => _yAxisPointStatus;
        set
        {
            _yAxisPointStatus = value;
            OnPropertyChanged();
        }
    }

    private bool _isRobotConnected = false;
    public bool IsRobotConnected
    {
        get => _isRobotConnected;
        set
        {
            _isRobotConnected = value;
            OnPropertyChanged();
        }
    }

    private bool _isCapturing = false;
    public bool IsCapturing
    {
        get => _isCapturing;
        set
        {
            _isCapturing = value;
            OnPropertyChanged();
        }
    }

    private bool _isOriginCaptured = false;
    public bool IsOriginCaptured
    {
        get => _isOriginCaptured;
        set
        {
            _isOriginCaptured = value;
            OnPropertyChanged();
        }
    }

    private bool _isXAxisCaptured = false;
    public bool IsXAxisCaptured
    {
        get => _isXAxisCaptured;
        set
        {
            _isXAxisCaptured = value;
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
        await ConnectToRobotAsync();
    }

    public async Task<(bool Success, string Message)> ConnectToRobotAsync()
    {
        try
        {
            await _configController.LoadConfigurationAsync();
            var ipAddress = _configController.GetRobotIpAddress();
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                IsRobotConnected = false;
                return (false, "No robot IP configured");
            }

            _urController = new URController(ipAddress);
            bool connected = await _urController.ConnectAsync();
            
            if (connected)
            {
                InitializeCalibrationController();
                IsRobotConnected = true;
                return (true, "Robot connected");
            }
            else
            {
                IsRobotConnected = false;
                _urController = null;
                return (false, "Failed to connect to robot");
            }
        }
        catch (Exception ex)
        {
            IsRobotConnected = false;
            _urController = null;
            return (false, $"Connection error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> SaveRobotIpAddressAsync()
    {
        try
        {
            bool success = await _configController.SetRobotIpAddressAsync(RobotIpAddress);
            if (!success)
            {
                StatusMessage = "Failed to save";
                return (false, "Failed to save IP address");
            }
            
            StatusMessage = "Robot IP saved, reconnecting...";
            
            // Disconnect existing connection if any
            if (_urController != null)
            {
                IsRobotConnected = false;
                _urController = null;
                _calibrationController = null;
                await Task.Delay(500); // Give time for disconnection
            }
            
            // Reconnect with new IP
            var result = await ConnectToRobotAsync();
            return result;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            return (false, $"Error: {ex.Message}");
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

    private void InitializeCalibrationController()
    {
        if (_urController == null || _calibrationController != null)
            return;
            
        var coordinateService = new CoordinateService();
        var storageService = new StorageService();
        _calibrationController = new CalibrationController(_urController, coordinateService, storageService);
    }

    public async Task<(bool Success, string Message)> CaptureOriginPointAsync()
    {
        if (IsCapturing)
            return (false, "Already capturing a point. Please wait.");

        IsCapturing = true;
        try
        {
            if (!IsRobotConnected)
                return (false, "Robot not connected. Check connection status above.");
                
            if (_calibrationController == null)
                return (false, "Calibration controller not initialized. Try reconnecting.");

            // Add 5-second timeout
            var captureTask = _calibrationController.GetCurrentPositionAsync("Origin");
            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(captureTask, timeoutTask);
            
            if (completedTask == timeoutTask)
                return (false, "Timeout: Robot did not respond within 5 seconds. Check robot status.");
            
            _originPoint = await captureTask;
            
            if (_originPoint == null)
                return (false, "Failed to read robot position. Verify robot is powered on and responding.");

            if (_originPoint.CartesianPositions == null)
                return (false, "Failed to get position data from robot.");

            var validation = _calibrationController.ValidateCalibrationPoint(_originPoint);
            if (!validation.IsValid)
                return (false, validation.ErrorMessage ?? "Invalid point");

            OriginPointStatus = $"Captured: ({_originPoint.CartesianPositions?.X:F3}, {_originPoint.CartesianPositions?.Y:F3}, {_originPoint.CartesianPositions?.Z:F3})";
            IsOriginCaptured = true;
            return (true, "Origin point captured");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
        finally
        {
            IsCapturing = false;
        }
    }

    public async Task<(bool Success, string Message)> CaptureXAxisPointAsync()
    {
        if (IsCapturing)
            return (false, "Already capturing a point. Please wait.");

        IsCapturing = true;
        try
        {
            if (!IsRobotConnected)
                return (false, "Robot not connected. Check connection status above.");
                
            if (_calibrationController == null)
                return (false, "Calibration controller not initialized. Try reconnecting.");

            // Add 5-second timeout
            var captureTask = _calibrationController.GetCurrentPositionAsync("X-Axis");
            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(captureTask, timeoutTask);
            
            if (completedTask == timeoutTask)
                return (false, "Timeout: Robot did not respond within 5 seconds. Check robot status.");
            
            _xAxisPoint = await captureTask;
            
            if (_xAxisPoint == null)
                return (false, "Failed to read robot position. Verify robot is powered on and responding.");

            if (_xAxisPoint.CartesianPositions == null)
                return (false, "Failed to get position data from robot.");

            var validation = _calibrationController.ValidateCalibrationPoint(_xAxisPoint, _originPoint);
            if (!validation.IsValid)
                return (false, validation.ErrorMessage ?? "Invalid point");

            var distance = _originPoint != null ? _calibrationController.CalculateDistance(_originPoint, _xAxisPoint) : 0;
            XAxisPointStatus = $"Captured: Distance from origin = {distance:F1}mm";
            IsXAxisCaptured = true;
            return (true, "X-Axis point captured");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
        finally
        {
            IsCapturing = false;
        }
    }

    public async Task<(bool Success, string Message)> CaptureYAxisPointAsync()
    {
        if (IsCapturing)
            return (false, "Already capturing a point. Please wait.");

        IsCapturing = true;
        try
        {
            if (!IsRobotConnected)
                return (false, "Robot not connected. Check connection status above.");
                
            if (_calibrationController == null)
                return (false, "Calibration controller not initialized. Try reconnecting.");

            // Add 5-second timeout
            var captureTask = _calibrationController.GetCurrentPositionAsync("Y-Axis");
            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(captureTask, timeoutTask);
            
            if (completedTask == timeoutTask)
                return (false, "Timeout: Robot did not respond within 5 seconds. Check robot status.");
            
            _yAxisPoint = await captureTask;
            
            if (_yAxisPoint == null)
                return (false, "Failed to read robot position. Verify robot is powered on and responding.");

            if (_yAxisPoint.CartesianPositions == null)
                return (false, "Failed to get position data from robot.");

            var validation = _calibrationController.ValidateCalibrationPoint(_yAxisPoint, _originPoint);
            if (!validation.IsValid)
                return (false, validation.ErrorMessage ?? "Invalid point");

            var distance = _originPoint != null ? _calibrationController.CalculateDistance(_originPoint, _yAxisPoint) : 0;
            YAxisPointStatus = $"Captured: Distance from origin = {distance:F1}mm";
            return (true, "Y-Axis point captured");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
        finally
        {
            IsCapturing = false;
        }
    }

    public async Task<(bool Success, string Message)> PerformCalibrationAsync()
    {
        try
        {
            if (!IsRobotConnected || _calibrationController == null)
                return (false, "Robot not connected");

            if (_originPoint == null || _xAxisPoint == null || _yAxisPoint == null)
                return (false, "Please capture all three calibration points first");

            var result = _calibrationController.CalibrateFromWaypoints(_originPoint, _xAxisPoint, _yAxisPoint);
            if (!result.Success)
                return (false, result.ErrorMessage ?? "Calibration failed");

            bool saved = await _calibrationController.SaveCalibrationAsync();
            if (!saved)
                return (false, "Calibration succeeded but failed to save");

            await LoadConfigurationAsync();
            return (true, "Calibration completed and saved successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
