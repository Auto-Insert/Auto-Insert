using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;
namespace AutoInsert.Core.Controllers;

public class ScrewingStationController
{
    private readonly LinearActuatorService LinearActuatorService = new();

    public ExecutableStatus ExtendScrewdriver(int percentage)
    {
        return LinearActuatorService.SetPosition(percentage);
    }
}