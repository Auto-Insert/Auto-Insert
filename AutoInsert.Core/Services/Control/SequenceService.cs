using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Core.Services.Control.StepHandlers;
using AutoInsert.Core.Services.Data;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control;

public class SequenceService()
{
    private ConfigurationController? _configurationController;
    private URController? _urController;
    private ScrewingStationController? _screwingStationController;
    private CoordinateService? _coordinateService;
    private StorageService? _storageService;
    private AppConfiguration? _appConfiguration;
    private Sequence? _currentSequence;
    private UartService? _uartService;
    private StepperMotorService? _stepperMotorService;
    private ServoMotorService? _servoMotorService;
    private SolenoidActuatorService? _solenoidActuatorService;
    private LinearActuatorService? _linearActuatorService;
    private StepFactory? _stepFactory;
    private CancellationTokenSource? _cancellationTokenSource;
    public event EventHandler<SequenceStep>? StepStarted;
    public event EventHandler<SequenceStep>? StepCompleted;
    public event EventHandler<SequenceStep>? StepFailed;
    public event EventHandler<Sequence>? SequenceCompleted;
    public event EventHandler<Sequence>? SequenceFailed;
    private bool _connectedToUr = false;
    private bool _connectedToUart = false;
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
        var serialPort = _appConfiguration.SerialPort;
        var baudRate = _appConfiguration.SerialBaudRate;
        if (string.IsNullOrEmpty(serialPort) || baudRate == null)
        {
            throw new InvalidOperationException("UART configuration not set");
        }

        // Initialize all controllers and services
        //_urController = new URController(robotIpAddress);
        _coordinateService = new CoordinateService();
        _screwingStationController = new ScrewingStationController();
        _uartService = new UartService();
        _stepperMotorService = new StepperMotorService(_uartService);
        _servoMotorService = new ServoMotorService(_uartService);
        _solenoidActuatorService = new SolenoidActuatorService(_uartService);
        _linearActuatorService = new LinearActuatorService();

        _stepFactory = new StepFactory(
            _urController!,
            _coordinateService,
            _uartService,
            _stepperMotorService,
            _servoMotorService,
            _solenoidActuatorService,
            _linearActuatorService);
        
        // Load calibration data if available
        if (_appConfiguration.CalibrationData != null)
        {
            _coordinateService.SetCalibrationData(_appConfiguration.CalibrationData);
        }
        
        // Connect to robot
        //_connectedToUr = await _urController.ConnectAsync();
        if (!_connectedToUr)
        {
            Console.WriteLine($"WARNING: Failed to connect to UR robot at {robotIpAddress}");
        }
        // Initialize UART service
        _connectedToUart = _uartService.Connect(serialPort, baudRate.Value);
        if (!_connectedToUart)
        {
             Console.WriteLine($"WARNING: Failed to connect to UART serial port. {serialPort} @ {baudRate}");
        }
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
    public async Task<bool> ExecuteSequenceAsync(Sequence? sequence = null)
    {
        if (_coordinateService == null || _screwingStationController == null)
        {
            throw new InvalidOperationException("Service not initialized");
        }
        if (sequence != null)
            _currentSequence = sequence;
        if (_currentSequence == null)
        {
            throw new InvalidOperationException("No sequence loaded to execute");
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

                var executableStep = _stepFactory!.CreateHandler(stepData);
                if (executableStep == null)
                {
                    stepData.Status = StepStatus.Failed;
                    stepData.ErrorMessage = $"No handler found for step type {stepData.StepType}";
                    StepFailed?.Invoke(this, stepData);
                    SequenceFailed?.Invoke(this, _currentSequence);
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
                    StepFailed?.Invoke(this, stepData);
                    SequenceFailed?.Invoke(this, _currentSequence);
                    return false;
                }
            }

            SequenceCompleted?.Invoke(this, _currentSequence);
            return true;
        }
        catch (Exception)
        {
            SequenceFailed?.Invoke(this, _currentSequence);
            return false;
        }
    }
    public void CancelSequence()
    {
        _cancellationTokenSource?.Cancel();
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
}