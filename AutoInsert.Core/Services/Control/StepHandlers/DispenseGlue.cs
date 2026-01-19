using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class DispenseGlue : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly LinearActuatorService? _linearActuatorService;
    public StepperMotorService.Direction Direction { get; set; }
    public DispenseGlue()
    {
        StepType = StepType.DispenseGlue;
    }
    public DispenseGlue(UartService uartService, StepperMotorService stepperMotorService, LinearActuatorService linearActuatorService)
    {
        StepType = StepType.DispenseGlue;

        _uartService = uartService;
        _stepperMotorService = stepperMotorService;
        _linearActuatorService = linearActuatorService;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();

        // Move to glue dispenser
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise,798);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Stepper 1 moved 798 steps<<<<");
        _linearActuatorService!.SetPosition(20);

        // Dispense glue
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        await _uartService!.SendCommandBufferAsync();

        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");
        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");
        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");
        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");

        CompleteStep(true, "Glue dispensed.");
    }
}