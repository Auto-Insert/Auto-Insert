using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class ScrewPlug : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly LinearActuatorService? _linearActuatorService;
    public ScrewPlug()
    {
        StepType = StepType.ScrewPlug;
    }
    public ScrewPlug(UartService uartService, StepperMotorService stepperMotorService, LinearActuatorService linearActuatorService)
    {
        StepType = StepType.ScrewPlug;

        _uartService = uartService;
        _stepperMotorService = stepperMotorService;
        _linearActuatorService = linearActuatorService;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();

        // Go to screwing position and screw.
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 4000);
        await _uartService!.AddDelayToBufferAsync(1);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 1500);
        await _uartService!.SendCommandBufferAsync();

        await _uartService!.WaitForStringAsync(">>>>INTERUPT");
        _linearActuatorService!.SetPosition(100);
        Task.Delay(5000).Wait();

        _linearActuatorService!.SetPosition(0);
        Task.Delay(5000).Wait();
        
        CompleteStep(true, "Plug screwed in.");
    }
}