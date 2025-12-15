using AutoInsert.Shared.Models;
using AutoInsert.Core.Services.Communication;

namespace AutoInsert.Core.Controllers;

// This class is for controlling motors and the UR directly for debugging purposes.
public class DebugController
{
    public string URIPAddress { get; } 
    private readonly URDashboardClient _dashboardClient;
    private readonly URPrimaryClient _primaryClient;
    private readonly URSecondaryClient _secondaryClient;
    private readonly ServoMotorService _servoMotorService;
    private readonly StepperMotorService _stepperMotorService;
    private readonly SolenoidActuatorService _solenoidActuatorService;
    private readonly UartService _uartService;
    private readonly LinearActuatorService _linearActuatorService;
    private readonly string _serialPort;

    public DebugController(string urIPAddress, string serialPort)
    {
        // Initialize UR clients
        URIPAddress = urIPAddress;
        _dashboardClient = new URDashboardClient(urIPAddress);
        _primaryClient = new URPrimaryClient(urIPAddress);
        _secondaryClient = new URSecondaryClient(urIPAddress);
        // Initialize UART service and motor services
        _uartService = new UartService();
        _serialPort = serialPort;
        _servoMotorService = new ServoMotorService(_uartService);
        _stepperMotorService = new StepperMotorService(_uartService);
        _solenoidActuatorService = new SolenoidActuatorService(_uartService);
        // Initialize LAC
        _linearActuatorService = new LinearActuatorService();
    }

    public async Task<bool> ConnectAsync()
    {
        var connectTasks = new[]
        {
            _dashboardClient.ConnectAsync(),
            _primaryClient.ConnectAsync(),
            _secondaryClient.ConnectAsync()
        };
        
        bool[] results = await Task.WhenAll(connectTasks);
        bool dashboardConnected = results[0];
        bool primaryConnected = results[1];
        bool secondaryConnected = results[2];
        //bool uartConnected = _uartService.Connect(_serialPort, baudRate: 115200);
        
        return dashboardConnected && primaryConnected && secondaryConnected; // && uartConnected;
    }

    public void Disconnect()
    {
        _dashboardClient.Disconnect();
        _primaryClient.Disconnect();
        _secondaryClient.Disconnect();
        _uartService.Disconnect();
    }
    
    #region UR Robot Control
    public async Task<string> GetRobotModeAsync()
    {
        string response = await _dashboardClient.SendCommandAsync("robotmode");
        return response.Replace("Robotmode:", "").Trim();
    }
    public async Task EnableFreedriveAsync()
    {
        string enableScript = @"
def freedrive_enable():
    freedrive_mode()
    while (True):
        sleep(0.1)
    end
end
";
        await _primaryClient.SendURScriptAsync(enableScript);   
    }
    public async Task DisableFreedriveAsync()
    {
        string disableScript = "end_freedrive_mode()";
        await _primaryClient.SendURScriptAsync(disableScript);
    }
    public async Task<Waypoint?> GetWaypointFromCurrentPositionsAsync(string? name = null)
    {
        Waypoint waypoint = new()
        {
            Name = name,
            JointPositions = await _secondaryClient.GetJointPositionsAsync(),
            CartesianPositions = await _secondaryClient.GetCartesianPositionsAsync()
        };
        return waypoint;
    }
    public async Task<CartesianPositions?> GetCurrentCartesianPositionsAsync()
    {
        return await _secondaryClient.GetCartesianPositionsAsync();
    }
    public async Task<double[]?> GetCurrentJointPositionsAsync()
    {
        return await _secondaryClient.GetJointPositionsAsync();
    }
    public async Task<ToolData?> GetToolDataAsync()
    {
        return await _secondaryClient.GetToolDataAsync();
    }
    public async Task MoveToJointPositions(double[] jointPositions, double speed, double acceleration)
    {
        var positions = string.Join(", ", jointPositions.Select(p => p.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
        
        string moveScript = $@"
    def move_to_position():
        movej([{positions}], a={acceleration.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, v={speed.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)})
    end
    ";
        await _primaryClient.SendURScriptAsync(moveScript);
    }
    public async Task<bool> SendURScriptAsync(string script) => 
        await _primaryClient.SendURScriptAsync(script);
    
    #endregion
    #region Motors via UART
    
    public static async Task<string[]> GetAvailableSerialPortsAsync()
    {
        return await Task.Run(() => UartService.GetAvailablePorts());
    }
    public async Task<MoveStatus> MoveServoMotorAsync(int degrees)
    {
        return await _servoMotorService.MoveAsync(degrees);
    }
    public async Task<MoveStatus> MoveStepperMotorAsync(StepperMotorService.Motor motor,StepperMotorService.Direction direction, int steps)
    {
        return await _stepperMotorService.MoveAsync(motor, direction, steps);
    }
    public async Task<MoveStatus> MoveSolenoidActuatorAsync(int actuator, SolenoidActuatorService.ActuatorMovement movement)
    {
        return await _solenoidActuatorService.MoveAsync(actuator, movement);
    }

    #endregion
    #region LAC Control
    
    public async Task<ExecutableStatus> ExtendScrewdriverAsync(int percentage)
    {
        return await Task.Run(() => _linearActuatorService.SetPosition(percentage));
    }
    
    #endregion
}