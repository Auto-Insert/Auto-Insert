using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;
using AutoInsert.Core.Services.Communication;

namespace AutoInsert.UI.ViewModels;

public class DebugViewModel : INotifyPropertyChanged
{
    private DebugController debugController;
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _toolDataCancellationTokenSource;
    private string _ipAddress = "192.168.0.108";
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged();
        }
    }
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
    private string _selectedSerialPort = "COM6";
    public string SelectedSerialPort
    {
        get => _selectedSerialPort;
        set
        {
            _selectedSerialPort = value;
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
        debugController = new DebugController(IpAddress, _selectedSerialPort);
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
            // Stop polling
            StopAllPolling();

            // Create new controller with updated IP
            debugController = new DebugController(_ipAddress, _selectedSerialPort);

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
                    
                    // Update on UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentPosition = position;
                    });

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
                    
                    // Update on UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentToolData = toolData;
                    });

                    await Task.Delay(150, _toolDataCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
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

    // UART Motors Control
    public async Task LoadAvailableSerialPortsAsync()
    {
        try
        {
            var ports = await DebugController.GetAvailableSerialPortsAsync();
            AvailableSerialPorts.Clear();
            foreach (var port in ports)
            {
                AvailableSerialPorts.Add(port);
            }
        }
        catch (Exception ex)
        {
            ScriptStatus = $"Error loading serial ports: {ex.Message}";
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