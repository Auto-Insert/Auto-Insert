using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class ResetRail : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly LinearActuatorService? _linearActuatorService;
    public StepperMotorService.Direction Direction { get; set; }
    public ResetRail()
    {
        StepType = StepType.ResetRail;
    }
    public ResetRail(UartService uartService, StepperMotorService stepperMotorService, LinearActuatorService linearActuatorService, StepperMotorService.Direction direction)
    {
        StepType = StepType.ResetRail;

        Direction = direction;
        _uartService = uartService;
        _stepperMotorService = stepperMotorService;
        _linearActuatorService = linearActuatorService;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();

        _linearActuatorService!.SetPosition(0);
        Task.Delay(5000).Wait();
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, Direction, 4000);
        await _uartService!.SendCommandBufferAsync();

        await _uartService!.WaitForStringAsync(">>>>INTERUPT");

        CompleteStep(true, "Rail has been reset.");
    }
}