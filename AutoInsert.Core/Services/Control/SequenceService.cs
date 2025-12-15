using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Control.StepHandlers;
using AutoInsert.Core.Services.Data;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control;

public class SequenceService()
{
    private ConfigurationController? _configurationController;
    private URController? _urController;
    private CalibrationController? _calibrationController;
    private ScrewingStationController? _screwingStationController;
    private CoordinateService? _coordinateService;
    private StorageService? _storageService;
    private AppConfiguration? _appConfiguration;
    private Sequence? _currentSequence;
    private CancellationTokenSource? _cancellationTokenSource;
    public event EventHandler<SequenceStep>? StepStarted;
    public event EventHandler<SequenceStep>? StepCompleted;
    public event EventHandler<(SequenceStep step, string errorMessage)>? StepFailed;
    public event EventHandler<Sequence>? SequenceCompleted;
    public event EventHandler<(Sequence Sequence, string Error)>? SequenceFailed;
    public async Task InitializeAsync()
    {
        _storageService = new StorageService();
        _configurationController = new ConfigurationController(_storageService);
        _appConfiguration = await _configurationController.LoadConfigurationAsync();
        
        var robotIpAddress = _appConfiguration.URRobotIpAddress;
        if (string.IsNullOrEmpty(robotIpAddress))
        {
            throw new InvalidOperationException("Robot IP address not configured");
        }
        
        // Initialize all controllers and services
        _urController = new URController(robotIpAddress);
        _coordinateService = new CoordinateService();
        _calibrationController = new CalibrationController(_urController, _coordinateService, _storageService);
        _screwingStationController = new ScrewingStationController();
        
        // Load calibration data if available
        if (_appConfiguration.CalibrationData != null)
        {
            _coordinateService.SetCalibrationData(_appConfiguration.CalibrationData);
        }
        
        // Connect to robot
        await _urController.ConnectAsync();
    }
    
    // Sequence management methods
    public void LoadSequence(Sequence sequence)
    {
        _currentSequence = sequence;
        
        // Reset all steps
        foreach (var step in _currentSequence.Steps)
        {
            step.Status = StepStatus.NotStarted;
            step.StartTime = null;
            step.EndTime = null;
            step.ErrorMessage = null;
        }
    }
    public async Task<bool> ExecuteSequenceAsync()
    {
        if (_currentSequence == null)
        {
            throw new InvalidOperationException("No sequence loaded");
        }

        if (_urController == null || _coordinateService == null || _screwingStationController == null)
        {
            throw new InvalidOperationException("Service not initialized. Call InitializeAsync first.");
        }

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            foreach (var stepData in _currentSequence.Steps)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return false;
                }

                // Convert data step to executable handler
                SequenceStep? executableStep = null;
                
                if (stepData is MoveURToPositionStep moveStep)
                {
                    if (moveStep.TargetPosition == null)
                    {
                        stepData.Status = StepStatus.Failed;
                        stepData.ErrorMessage = "Target position is null";
                        StepFailed?.Invoke(this, (stepData, stepData.ErrorMessage));
                        SequenceFailed?.Invoke(this, (_currentSequence, stepData.ErrorMessage));
                        return false;
                    }
                    
                    executableStep = new MoveURToPosition(
                        _urController,
                        _coordinateService,
                        moveStep.TargetPosition,
                        moveStep.Speed,
                        moveStep.Acceleration,
                        moveStep.GripPart)
                    {
                        Name = moveStep.Name,
                        Description = moveStep.Description
                    };
                }
                else if (stepData is SetScrewdriverExtensionStep screwStep)
                {
                    executableStep = new SetScrewdriverExtension(
                        _screwingStationController,
                        screwStep.Percentage)
                    {
                        Name = screwStep.Name,
                        Description = screwStep.Description
                    };
                }
                
                if (executableStep == null)
                {
                    stepData.Status = StepStatus.Failed;
                    stepData.ErrorMessage = $"Unknown step type: {stepData.StepType}";
                    StepFailed?.Invoke(this, (stepData, stepData.ErrorMessage));
                    SequenceFailed?.Invoke(this, (_currentSequence, stepData.ErrorMessage));
                    return false;
                }

                StepStarted?.Invoke(this, stepData);
                
                await executableStep.ExecuteAsync();
                
                // Copy execution result back to data step
                stepData.Status = executableStep.Status;
                stepData.StartTime = executableStep.StartTime;
                stepData.EndTime = executableStep.EndTime;
                stepData.ErrorMessage = executableStep.ErrorMessage;
                
                if (stepData.Status == StepStatus.Completed)
                {
                    StepCompleted?.Invoke(this, stepData);
                }
                else if (stepData.Status == StepStatus.Failed)
                {
                    StepFailed?.Invoke(this, (stepData, stepData.ErrorMessage));
                    SequenceFailed?.Invoke(this, (_currentSequence, stepData.ErrorMessage ?? "Step failed"));
                    return false;
                }
            }

            SequenceCompleted?.Invoke(this, _currentSequence);
            return true;
        }
        catch (Exception ex)
        {
            SequenceFailed?.Invoke(this, (_currentSequence, ex.Message));
            return false;
        }
    }
    public void CancelSequence()
    {
        _cancellationTokenSource?.Cancel();
    }

    // Sequence creation and step adding
    public void CreateNewSequence(string name, string? description = null)
    {
        _currentSequence = new Sequence
        {
            Name = name,
            Description = description,
            Steps = new List<SequenceStep>()
        };
    }

    // Sequence persistence methods
    public async Task<bool> SaveCurrentSequenceAsync()
    {
        if (_currentSequence == null)
        {
            throw new InvalidOperationException("No sequence loaded to save");
        }

        if (_storageService == null)
        {
            throw new InvalidOperationException("Service not initialized");
        }

        return await _storageService.SaveSequenceAsync(_currentSequence);
    }

    public async Task<Sequence?> LoadSequenceFromStorageAsync(string sequenceName)
    {
        if (_storageService == null)
        {
            throw new InvalidOperationException("Service not initialized");
        }

        return await _storageService.LoadSequenceAsync(sequenceName);
    }

    public async Task<List<Sequence>> LoadAllSequencesAsync()
    {
        if (_storageService == null)
        {
            throw new InvalidOperationException("Service not initialized");
        }

        return await _storageService.LoadAllSequencesAsync();
    }

    public bool DeleteSequence(string sequenceName)
    {
        if (_storageService == null)
        {
            throw new InvalidOperationException("Service not initialized");
        }

        return _storageService.DeleteSequence(sequenceName);
    }

    public List<string> GetSavedSequenceNames()
    {
        if (_storageService == null)
        {
            throw new InvalidOperationException("Service not initialized");
        }

        return _storageService.GetSequenceNames();
    }

    public Sequence? GetCurrentSequence() => _currentSequence;

    public URController? GetURController() => _urController;
    public CoordinateService? GetCoordinateService() => _coordinateService;
    public ScrewingStationController? GetScrewingStationController() => _screwingStationController;
}
