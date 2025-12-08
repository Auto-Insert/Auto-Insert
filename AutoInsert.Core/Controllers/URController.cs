using AutoInsert.Shared.Models;
using AutoInsert.Core.Services.Communication;

namespace AutoInsert.Core.Controllers;

public class URController(UR ur)
{
    public UR UR { get; } = ur;
    private readonly URDashboardClient _dashboardClient = new(ur.IPAddress);
    private readonly URPrimaryClient _primaryClient = new(ur.IPAddress);
    private readonly URSecondaryClient _secondaryClient = new(ur.IPAddress);
    public async Task<bool> ConnectAsync()
    {
        bool dashboardConnected = await _dashboardClient.ConnectAsync();
        bool primaryConnected = await _primaryClient.ConnectAsync();
        bool secondaryConnected = await _secondaryClient.ConnectAsync();
        return dashboardConnected && primaryConnected && secondaryConnected;
    }
    
    // Robot State
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
    
    // Get waypoint
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

    // Get current cartesian positions
    public async Task<CartesianPositions?> GetCurrentCartesianPositionsAsync()
    {
        return await _secondaryClient.GetCartesianPositionsAsync();
    }

    // Get current joint positions
    public async Task<double[]?> GetCurrentJointPositionsAsync()
    {
        return await _secondaryClient.GetJointPositionsAsync();
    }

    // Get tool data
    public async Task<ToolData?> GetToolDataAsync()
    {
        return await _secondaryClient.GetToolDataAsync();
    }

    // Move commands
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

    // URScript 
    public async Task<bool> SendURScriptAsync(string script) => 
        await _primaryClient.SendURScriptAsync(script);
}