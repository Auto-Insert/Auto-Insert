using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.Services;
public class ScrewingStationService
{
    private static readonly ScrewingStationController _lacController = new();
    public static async Task<ExecutableStatus> ExtendScrewdriverAsync(int percentage)
    {
        return await Task.Run(() =>
        {
            return _lacController.ExtendScrewdriver(percentage);
        });
    }
}