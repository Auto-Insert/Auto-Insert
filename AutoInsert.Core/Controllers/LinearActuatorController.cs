using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;
namespace AutoInsert.Core.Controllers;

public class LinearActuatorController()
{
    private readonly LinearActuatorService LinearActuatorService = new();

    public ExecutableStatus SetPosition(int percentage)
    {
        return LinearActuatorService.SetPosition(percentage);
    }
}