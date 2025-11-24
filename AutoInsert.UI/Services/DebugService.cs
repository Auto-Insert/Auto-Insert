using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.Services;

public class DebugService
{
    private readonly URController _urController;

    public DebugService()
    {
        var ur = new UR("192.168.0.108");
        _urController = new URController(ur);
    }

    public DebugService(UR ur)
    {
        _urController = new URController(ur);
    }

    // Connection
    public async Task<bool> ConnectAsync()
    {
        return await _urController.ConnectAsync();
    }

    // Robot State
    public async Task<string> GetRobotModeAsync()
    {
        return await _urController.GetRobotModeAsync();
    }

    // Freedrive Mode
    public async Task EnableFreedriveAsync()
    {
        await _urController.EnableFreedriveAsync();
    }

    public async Task DisableFreedriveAsync()
    {
        await _urController.DisableFreedriveAsync();
    }

    // Position and Tool Data
    public async Task<Waypoint?> GetCurrentPositionAsync()
    {
        return await _urController.GetCurrentPositionAsync();
    }

    public async Task<ToolData?> GetToolDataAsync()
    {
        return await _urController.GetToolDataAsync();
    }

    // URScript Execution
    public async Task<bool> SendURScriptAsync(string script)
    {
        return await _urController.SendURScriptAsync(script);
    }
}