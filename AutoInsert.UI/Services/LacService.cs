using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.Services;
public class LacService
{
    private static readonly LinearActuatorController _lacController = new();
    public static async Task<ExecutableStatus> SetPositionAsync(int percentage)
    {
        return await Task.Run(() =>
        {
            return _lacController.SetPosition(percentage);
        });
    }
}