using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class DetachTool : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly LinearActuatorService? _linearActuatorService;
    public StepperMotorService.Direction Direction { get; set; }
    public DetachTool()
    {
        StepType = StepType.DetachTool;
    }
    public DetachTool(UartService uartService, StepperMotorService stepperMotorService, LinearActuatorService linearActuatorService)
    {
        StepType = StepType.DetachTool;

        _uartService = uartService;
        _stepperMotorService = stepperMotorService;
        _linearActuatorService = linearActuatorService;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();

        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 17);
        _linearActuatorService!.SetPosition(100);
        await _uartService!.SendCommandBufferAsync();

        await _uartService!.WaitForStringAsync(">>>>Stepper 3 moved 17 steps<<<<");
        _linearActuatorService!.SetPosition(0);
        Task.Delay(1000).Wait();
        CompleteStep(true, "Tool deattached.");
    }
}