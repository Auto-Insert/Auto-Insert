using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Core.Services.Data;
using System.Linq;

namespace AutoInsert.UI.ViewModels;

public class DebugViewModel : INotifyPropertyChanged
{
    private readonly ConfigurationController _configController;
    private DebugController debugController;
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _toolDataCancellationTokenSource;
    
    private string? _robotMode = "Not Connected";
    public string? RobotMode
    {
        get => _robotMode;
        set
        {
            _robotMode = value;
            OnPropertyChanged();
        }
    }

    private string _configuredIpAddress = "192.168.0.108";
    public string ConfiguredIpAddress
    {
        get => _configuredIpAddress;
        set
        {
            _configuredIpAddress = value;
            OnPropertyChanged();
        }
    }

    private string _configuredSerialPort = "COM6";
    public string ConfiguredSerialPort
    {
        get => _configuredSerialPort;
        set
        {
            _configuredSerialPort = value;
            OnPropertyChanged();
        }
    }

    private int _configuredBaudRate = 115200;
    public int ConfiguredBaudRate
    {
        get => _configuredBaudRate;
        set
        {
            _configuredBaudRate = value;
            OnPropertyChanged();
        }
    }
    private double[]? _currentPosition;
    public double[]? CurrentPosition
    {
        get => _currentPosition;
        set
        {
            _currentPosition = value;
            OnPropertyChanged();
        }
    }
    private ToolData? _currentToolData;
    public ToolData? CurrentToolData
    {
        get => _currentToolData;
        set
        {
            _currentToolData = value;
            OnPropertyChanged();
        }
    }
    private string _customScript = "";
    public string CustomScript
    {
        get => _customScript;
        set
        {
            _customScript = value;
            OnPropertyChanged();
        }
    }
    private string _scriptStatus = "";
    public string ScriptStatus
    {
        get => _scriptStatus;
        set
        {
            _scriptStatus = value;
            OnPropertyChanged();
        }
    }
    private double _moveSpeed = 0.25;
    public double MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            _moveSpeed = value;
            OnPropertyChanged();
        }
    }
    private double _moveAcceleration = 1.2;
    public double MoveAcceleration
    {
        get => _moveAcceleration;
        set
        {
            _moveAcceleration = value;
            OnPropertyChanged();
        }
    }
    
    // Local waypoint properties
    private ObservableCollection<LocalWaypoint> _localWaypoints = new();
    public ObservableCollection<LocalWaypoint> LocalWaypoints
    {
        get => _localWaypoints;
        set
        {
            _localWaypoints = value;
            OnPropertyChanged();
        }
    }
    
    private LocalWaypoint? _selectedLocalWaypoint;
    public LocalWaypoint? SelectedLocalWaypoint
    {
        get => _selectedLocalWaypoint;
        set
        {
            _selectedLocalWaypoint = value;
            OnPropertyChanged();
            
            // Update input fields when waypoint is selected
            if (value != null)
            {
                LocalWaypointName = value.Name;
                LocalX = value.X;
                LocalY = value.Y;
                LocalZ = value.Z;
            }
        }
    }
    
    private string _localWaypointName = "";
    public string LocalWaypointName
    {
        get => _localWaypointName;
        set
        {
            _localWaypointName = value;
            OnPropertyChanged();
        }
    }
    
    private double _localX = 0;
    public double LocalX
    {
        get => _localX;
        set
        {
            _localX = value;
            OnPropertyChanged();
        }
    }
    
    private double _localY = 0;
    public double LocalY
    {
        get => _localY;
        set
        {
            _localY = value;
            OnPropertyChanged();
        }
    }
    
    private double _localZ = 0;
    public double LocalZ
    {
        get => _localZ;
        set
        {
            _localZ = value;
            OnPropertyChanged();
        }
    }
    
    private string _localWaypointStatus = "";
    public string LocalWaypointStatus
    {
        get => _localWaypointStatus;
        set
        {
            _localWaypointStatus = value;
            OnPropertyChanged();
        }
    }
    
    private string _calibrationStatusText = "";
    public string CalibrationStatusText
    {
        get => _calibrationStatusText;
        set
        {
            _calibrationStatusText = value;
            OnPropertyChanged();
        }
    }
    
    private double[]? _selectedWaypoint;
    public double[]? SelectedWaypoint
    {
        get => _selectedWaypoint;
        set
        {
            _selectedWaypoint = value;
            OnPropertyChanged();
        }
    }
    private ObservableCollection<double[]> _savedWaypoints = new();
    public ObservableCollection<double[]> SavedWaypoints {
        get => _savedWaypoints;
        set
        {
            _savedWaypoints = value;
            OnPropertyChanged();
        }
    }
    private int _extensionPercentage;
    public int ExtensionPercentage
    {
        get => _extensionPercentage;
        set
        {
            _extensionPercentage = value;
            OnPropertyChanged();
        }
    }
    private string? _extensionStatus;
    public string? ExtensionStatus
    {
        get => _extensionStatus;
        set
        {
            _extensionStatus = value;
            OnPropertyChanged();
        }
    }

    // UART Motor Control Properties
    private int _servoDegrees = 90;
    public int ServoDegrees
    {
        get => _servoDegrees;
        set
        {
            _servoDegrees = value;
            OnPropertyChanged();
        }
    }
    private string? _servoStatus;
    public string? ServoStatus
    {
        get => _servoStatus;
        set
        {
            _servoStatus = value;
            OnPropertyChanged();
        }
    }

    private int _stepperMotorSelection = 1; // 1=Rail, 2=Pump, 3=Tool
    public int StepperMotorSelection
    {
        get => _stepperMotorSelection;
        set
        {
            _stepperMotorSelection = value;
            OnPropertyChanged();
        }
    }

    private int _stepperDirection = 1; // 0=AntiClockwise, 1=Clockwise
    public int StepperDirection
    {
        get => _stepperDirection;
        set
        {
            _stepperDirection = value;
            OnPropertyChanged();
        }
    }

    private int _stepperSteps = 1000;
    public int StepperSteps
    {
        get => _stepperSteps;
        set
        {
            _stepperSteps = value;
            OnPropertyChanged();
        }
    }

    private string? _stepperStatus;
    public string? StepperStatus
    {
        get => _stepperStatus;
        set
        {
            _stepperStatus = value;
            OnPropertyChanged();
        }
    }

    private int _solenoidActuatorNumber = 1;
    public int SolenoidActuatorNumber
    {
        get => _solenoidActuatorNumber;
        set
        {
            _solenoidActuatorNumber = value;
            OnPropertyChanged();
        }
    }

    private int _solenoidMovement = 0; // 0=Extend, 1=Retract
    public int SolenoidMovement
    {
        get => _solenoidMovement;
        set
        {
            _solenoidMovement = value;
            OnPropertyChanged();
        }
    }

    private string? _solenoidStatus;
    public string? SolenoidStatus
    {
        get => _solenoidStatus;
        set
        {
            _solenoidStatus = value;
            OnPropertyChanged();
        }
    }
    
    public DebugViewModel()
    {
        _configController = new ConfigurationController(new StorageService());
        debugController = new DebugController("192.168.0.108", "COM6");
        
        // Initialize with default Zero Point waypoint
        LocalWaypoints.Add(new LocalWaypoint("Zero Point", 0, 0, 0));
        
        _ = LoadConfigurationAsync();
    }
    
    private async Task LoadConfigurationAsync()
    {
        try
        {
            await _configController.LoadConfigurationAsync();
            var ipAddress = _configController.GetRobotIpAddress() ?? "192.168.0.108";
            var serialPort = _configController.GetSerialPort() ?? "COM6";
            var baudRate = _configController.GetSerialBaudRate();
            
            ConfiguredIpAddress = ipAddress;
            ConfiguredSerialPort = serialPort;
            ConfiguredBaudRate = baudRate;
            
            debugController = new DebugController(ipAddress, serialPort);
            
            // Check calibration status
            if (_configController.HasCalibration())
            {
                var calibTime = _configController.GetLastCalibrationTime();
                CalibrationStatusText = calibTime.HasValue 
                    ? $"Calibration active (from {calibTime.Value:yyyy-MM-dd HH:mm})"
                    : "Calibration active";
            }
            else
            {
                CalibrationStatusText = "⚠ No calibration found. Configure calibration in Settings page to use local coordinates.";
            }
        }
        catch
        {
            // Keep default values
            CalibrationStatusText = "⚠ Error loading calibration status";
        }
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            bool connected = await debugController.ConnectAsync();
            
            if (connected)
            {
                RobotMode = await debugController.GetRobotModeAsync();
                
                StartPositionPolling();
                StartToolDataPolling();
            }
            else
            {
                RobotMode = "Not Connected";
            }
        }
        catch (Exception ex)
        {
            RobotMode = $"Error: {ex.Message}";
        }
    }

    public async Task ReconnectAsync()
    {
        try
        {
            StopAllPolling();
            try
            {
                debugController.Disconnect();
                await Task.Delay(500);
            }
            catch {}

            // Load configuration
            await _configController.LoadConfigurationAsync();
            var ipAddress = _configController.GetRobotIpAddress() ?? "192.168.0.108";
            var serialPort = _configController.GetSerialPort() ?? "COM6";
            var baudRate = _configController.GetSerialBaudRate();
            
            ConfiguredIpAddress = ipAddress;
            ConfiguredSerialPort = serialPort;
            ConfiguredBaudRate = baudRate;
            
            debugController = new DebugController(ipAddress, serialPort);

            // Connect
            ScriptStatus = "Connecting...";
            bool connected = await debugController.ConnectAsync();
            
            if (connected)
            {
                RobotMode = await debugController.GetRobotModeAsync();
                ScriptStatus = "Connected successfully";
                
                StartPositionPolling();
                StartToolDataPolling();
            }
            else
            {
                RobotMode = "Not Connected";
                ScriptStatus = "Connection failed";
            }
        }
        catch (Exception ex)
        {
            RobotMode = $"Error: {ex.Message}";
            ScriptStatus = $"Connection error: {ex.Message}";
        }
    }

    // UR Robot Control
    public async Task EnableFreedriveAsync()
    {
        try
        {
            await debugController.EnableFreedriveAsync();
            ScriptStatus = "Freedrive enabled";
        }
        catch (Exception ex)
        {
            ScriptStatus = $"Error: {ex.Message}";
        }
    }

    public async Task DisableFreedriveAsync()
    {
        try
        {
            await debugController.DisableFreedriveAsync();
            ScriptStatus = "Freedrive disabled";
        }
        catch (Exception ex)
        {
            ScriptStatus = $"Error: {ex.Message}";
        }
    }

    public async Task SendCustomScriptAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CustomScript))
            {
                ScriptStatus = "Please enter a script";
                return;
            }

            bool success = await debugController.SendURScriptAsync(CustomScript);
            ScriptStatus = success ? "Script sent successfully" : "Failed to send script";
        }
        catch (Exception ex)
        {
            ScriptStatus = $"Error: {ex.Message}";
        }
    }

    public void SaveCurrentWaypoint()
    {
        try
        {
            if (CurrentPosition == null)
            {
                ScriptStatus = "No current position available";
                return;
            }

            SavedWaypoints.Add(CurrentPosition);
            ScriptStatus = $"Waypoint saved (Total: {SavedWaypoints.Count})";
        }
        catch (Exception ex)
        {
            ScriptStatus = $"Error saving waypoint: {ex.Message}";
        }
    }

    public void DeleteSelectedWaypoint()
    {
        try
        {
            if (SelectedWaypoint == null)
            {
                ScriptStatus = "No waypoint selected";
                return;
            }

            SavedWaypoints.Remove(SelectedWaypoint);
            SelectedWaypoint = null;
            ScriptStatus = $"Waypoint deleted (Total: {SavedWaypoints.Count})";
        }
        catch (Exception ex)
        {
            ScriptStatus = $"Error deleting waypoint: {ex.Message}";
        }
    }

    public async Task MoveToSelectedWaypointAsync()
    {
        try
        {
            if (SelectedWaypoint == null)
            {
                ScriptStatus = "No waypoint selected";
                return;
            }

            ScriptStatus = $"Moving to waypoint (v={MoveSpeed}, a={MoveAcceleration})...";
            await debugController.MoveToJointPositions(SelectedWaypoint, MoveSpeed, MoveAcceleration);
            ScriptStatus = "Move command sent";
        }
        catch (Exception ex)
        {
            ScriptStatus = $"Error moving to waypoint: {ex.Message}";
        }
    }
    public void AddLocalWaypoint()
    {
        if (string.IsNullOrWhiteSpace(LocalWaypointName))
        {
            LocalWaypointStatus = "Please enter a waypoint name";
            return;
        }
        
        // Check for duplicate names
        if (LocalWaypoints.Any(w => w.Name.Equals(LocalWaypointName, StringComparison.OrdinalIgnoreCase)))
        {
            LocalWaypointStatus = $"Waypoint '{LocalWaypointName}' already exists";
            return;
        }
        
        var waypoint = new LocalWaypoint(LocalWaypointName, LocalX, LocalY, LocalZ);
        LocalWaypoints.Add(waypoint);
        LocalWaypointStatus = $"Added waypoint '{LocalWaypointName}' at X={LocalX:F1}mm, Y={LocalY:F1}mm, Z={LocalZ:F1}mm";
        
        // Clear inputs for next waypoint
        LocalWaypointName = "";
        LocalX = 0;
        LocalY = 0;
        LocalZ = 0;
    }
    
    public void DeleteLocalWaypoint()
    {
        if (SelectedLocalWaypoint == null)
        {
            LocalWaypointStatus = "Please select a waypoint to delete";
            return;
        }
        
        // Don't allow deleting Zero Point
        if (SelectedLocalWaypoint.Name == "Zero Point")
        {
            LocalWaypointStatus = "Cannot delete Zero Point";
            return;
        }
        
        string name = SelectedLocalWaypoint.Name;
        LocalWaypoints.Remove(SelectedLocalWaypoint);
        SelectedLocalWaypoint = null;
        LocalWaypointStatus = $"Deleted waypoint '{name}'";
        
        // Clear inputs
        LocalWaypointName = "";
        LocalX = 0;
        LocalY = 0;
        LocalZ = 0;
    }
    
    public async Task MoveToLocalWaypointAsync()
    {
        try
        {
            if (SelectedLocalWaypoint == null)
            {
                LocalWaypointStatus = "Please select a waypoint to move to";
                return;
            }
            
            // Check if connected
            if (debugController == null)
            {
                LocalWaypointStatus = "Error: Not connected to robot";
                return;
            }

            // Check calibration
            if (!_configController.HasCalibration())
            {
                LocalWaypointStatus = "No calibration found. Configure in Settings.";
                return;
            }

            LocalWaypointStatus = $"Moving to '{SelectedLocalWaypoint.Name}'...";

            // Get calibration data
            var config = _configController.GetConfiguration();
            var calibrationData = config.CalibrationData;
            
            if (calibrationData == null)
            {
                LocalWaypointStatus = "Failed to load calibration data";
                return;
            }

            // Create CoordinateService with calibration
            var coordinateService = new AutoInsert.Core.Services.Control.CoordinateService();
            coordinateService.SetCalibrationData(calibrationData);

            // Convert local coordinates (in mm) to meters for robot
            double localXMeters = SelectedLocalWaypoint.X / 1000.0;  // Convert mm to meters
            double localYMeters = SelectedLocalWaypoint.Y / 1000.0;
            double localZMeters = SelectedLocalWaypoint.Z / 1000.0;

            // Convert to robot coordinates
            var robotPosition = coordinateService.LocalToGlobal(localXMeters, localYMeters, localZMeters);

            // Move to the position using movel with the orientation from calibration
            string moveScript = $@"def move_to_local():
    target_pose = p[{robotPosition.X:F6}, {robotPosition.Y:F6}, {robotPosition.Z:F6}, {robotPosition.Rx:F6}, {robotPosition.Ry:F6}, {robotPosition.Rz:F6}]
    movel(target_pose, a={MoveAcceleration:F2}, v={MoveSpeed:F2})
end
";

            bool success = await debugController.SendURScriptAsync(moveScript);
            
            if (success)
            {
                LocalWaypointStatus = $"Moving to '{SelectedLocalWaypoint.Name}' (Robot: X={robotPosition.X:F3}m, Y={robotPosition.Y:F3}m, Z={robotPosition.Z:F3}m)";
            }
            else
            {
                LocalWaypointStatus = "Failed to send move command to robot";
            }
        }
        catch (Exception ex)
        {
            LocalWaypointStatus = $"Error: {ex.Message}";
        }
    }
    private void StartPositionPolling()
    {
        // Cancel any existing polling
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        // Start background task to poll position every 500ms
        _ = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var position = await debugController.GetCurrentJointPositionsAsync();
                    
                    // Update on UI thread only if we got valid data
                    if (position != null && position.Length > 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            CurrentPosition = position;
                        });
                    }

                    await Task.Delay(50, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    // Log error but continue polling
                    System.Diagnostics.Debug.WriteLine($"Error getting position: {ex.Message}");
                }
            }
        }, _cancellationTokenSource.Token);
    }

    private void StartToolDataPolling()
    {
        // Cancel any existing polling
        _toolDataCancellationTokenSource?.Cancel();
        _toolDataCancellationTokenSource = new CancellationTokenSource();

        // Start background task to poll tool data every second
        _ = Task.Run(async () =>
        {
            while (!_toolDataCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var toolData = await debugController.GetToolDataAsync();
                    
                    if (toolData != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            CurrentToolData = toolData;
                        });
                    }

                    await Task.Delay(150, _toolDataCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Log error but continue polling
                    System.Diagnostics.Debug.WriteLine($"Error getting tool data: {ex.Message}");
                }
            }
        }, _toolDataCancellationTokenSource.Token);
    }

    public void StopPositionPolling()
    {
        _cancellationTokenSource?.Cancel();
    }

    public void StopToolDataPolling()
    {
        _toolDataCancellationTokenSource?.Cancel();
    }

    public void StopAllPolling()
    {
        StopPositionPolling();
        StopToolDataPolling();
    }

    public void Disconnect()
    {
        StopAllPolling();
        try
        {
            debugController.Disconnect();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disconnecting: {ex.Message}");
        }
    }

    // LAC Control
    public async Task SetScrewdriverExtensionAsync()
    {
        try
        {
            ExtensionStatus = "Moving actuator...";
            
            var result = await debugController.ExtendScrewdriverAsync(ExtensionPercentage);
            
            if (result.Success)
            {
                ExtensionStatus = "Movement sent";
            }
            else
            {
                ExtensionStatus = $"Error: {result.Output}";
            }
        }
        catch (Exception ex)
        {
            ExtensionStatus = $"Error: {ex.Message}";
        }
    }

    public async Task MoveServoMotorAsync()
    {
        try
        {
            ServoStatus = $"Moving servo to {ServoDegrees}°...";
            var result = await debugController.MoveServoMotorAsync(ServoDegrees);
            ServoStatus = result.Success ? result.Message : $"Error: {result.Message}";
        }
        catch (Exception ex)
        {
            ServoStatus = $"Error: {ex.Message}";
        }
    }

    public async Task MoveStepperMotorAsync()
    {
        try
        {
            var motor = (StepperMotorService.Motor)StepperMotorSelection;
            var direction = (StepperMotorService.Direction)StepperDirection;
            
            StepperStatus = $"Moving {motor} motor {direction} {StepperSteps} steps...";
            var result = await debugController.MoveStepperMotorAsync(motor, direction, StepperSteps);
            StepperStatus = result.Success ? result.Message : $"Error: {result.Message}";
        }
        catch (Exception ex)
        {
            StepperStatus = $"Error: {ex.Message}";
        }
    }

    public async Task MoveSolenoidActuatorAsync()
    {
        try
        {
            var movement = (SolenoidActuatorService.ActuatorMovement)SolenoidMovement;
            
            SolenoidStatus = $"Moving actuator {SolenoidActuatorNumber} to {movement}...";
            var result = await debugController.MoveSolenoidActuatorAsync(SolenoidActuatorNumber, movement);
            SolenoidStatus = result.Success ? result.Message : $"Error: {result.Message}";
        }
        catch (Exception ex)
        {
            SolenoidStatus = $"Error: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}