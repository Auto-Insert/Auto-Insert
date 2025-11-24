using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Shared.Models;
using AutoInsert.UI.Services;

namespace AutoInsert.UI.ViewModels;

public class DebugViewModel : INotifyPropertyChanged
{
    private readonly DebugService debugService = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _toolDataCancellationTokenSource;
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
    
    public async Task InitializeAsync()
    {
        try
        {
            bool connected = await debugService.ConnectAsync();
            
            if (connected)
            {
                RobotMode = await debugService.GetRobotModeAsync();
                
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
                    var position = await debugService.GetCurrentPositionAsync();
                    
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
                    var toolData = await debugService.GetToolDataAsync();
                    
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