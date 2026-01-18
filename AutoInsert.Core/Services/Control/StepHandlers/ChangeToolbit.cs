using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class ChangeToolBit : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly LinearActuatorService? _linearActuatorService;
    public StepperMotorService.Direction Direction { get; set; }
    public ChangeToolBit()
    {
        StepType = StepType.ChangeToolBit;
    }
    public ChangeToolBit(UartService uartService, StepperMotorService stepperMotorService, LinearActuatorService linearActuatorService)
    {
        StepType = StepType.ChangeToolBit;

        _uartService = uartService;
        _stepperMotorService = stepperMotorService;
        _linearActuatorService = linearActuatorService;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();
        // Move to second bit.
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 222);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Stepper 1 moved 222 steps<<<<");

        // Spin while going up to attach the bit.
        _linearActuatorService!.SetPosition(100);
        Task.Delay(2000).Wait();
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.Clockwise, 12);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Stepper 3 moved 12 steps<<<<");
        _linearActuatorService!.SetPosition(0);
        Task.Delay(5000).Wait();

        CompleteStep(true, "Toolbit changed.");
    }
}