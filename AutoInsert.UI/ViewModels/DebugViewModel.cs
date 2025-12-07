using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Shared.Models;
using AutoInsert.UI.Services;

namespace AutoInsert.UI.ViewModels;

public class DebugViewModel : INotifyPropertyChanged
{
    private URService urService;
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _toolDataCancellationTokenSource;
    
    private string _ipAddress = "192.168.1.1";
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    private string? _robotMode = null;
    public string? RobotMode
    {
        get => _robotMode;
        set
        {
            _robotMode = value;
            OnPropertyChanged();
        }
    }
    
    private Waypoint? _currentPosition;
    public Waypoint? CurrentPosition
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

    private Waypoint? _selectedWaypoint;
    public Waypoint? SelectedWaypoint
    {
        get => _selectedWaypoint;
        set
        {
            _selectedWaypoint = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Waypoint> SavedWaypoints { get; } = new();

    public DebugViewModel()
    {
        var ur = new UR(IpAddress);
        urService = new URService(ur);
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            bool connected = await urService.ConnectAsync();
            
            if (connected)
            {
                RobotMode = await urService.GetRobotModeAsync();
                
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

            // Create new service with updated IP
            var ur = new UR(IpAddress);
            urService = new URService(ur);

            // Connect
            ScriptStatus = "Connecting...";
            bool connected = await urService.ConnectAsync();
            
            if (connected)
            {
                RobotMode = await urService.GetRobotModeAsync();
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

    public async Task EnableFreedriveAsync()
    {
        try
        {
            await urService.EnableFreedriveAsync();
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
            await urService.DisableFreedriveAsync();
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

            bool success = await urService.SendURScriptAsync(CustomScript);
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

            // Create a copy of the current position
            var waypoint = new Waypoint
            {
                JointPositions = CurrentPosition.JointPositions.ToArray()
            };

            SavedWaypoints.Add(waypoint);
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
            await urService.MoveToPositionAsync(SelectedWaypoint, MoveSpeed, MoveAcceleration);
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
                    var position = await urService.GetCurrentPositionAsync();
                    
                    // Update on UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentPosition = position;
                    });

                    await Task.Delay(150, _cancellationTokenSource.Token);
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
                    var toolData = await urService.GetToolDataAsync();
                    
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}