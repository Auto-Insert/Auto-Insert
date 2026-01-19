using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class DetachTool : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly LinearActuatorService? _linearActuatorService;
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

        _linearActuatorService!.SetPosition(80);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 25);
        await _uartService!.SendCommandBufferAsync();

        await _uartService!.WaitForStringAsync(">>>>INTERUPT, Completed: 0 / 25 steps");
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 25);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Stepper 3 moved 25 steps<<<<");        
        _linearActuatorService!.SetPosition(90);


        await _stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.Clockwise, 700);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Stepper 3 moved 700 steps<<<<");
        _linearActuatorService!.SetPosition(0);

        Task.Delay(3000).Wait();
        
        CompleteStep(true, "Tool deattached.");
    }
}