using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class ScrewInThePlug : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly ServoMotorService? _servoMotorService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly SolenoidActuatorService? _solenoidActuatorService;
    private readonly LinearActuatorService? _linearActuatorService;
    public string? PlugType { get; set; }
    public ScrewInThePlug()
    {
        StepType = StepType.ScrewInThePlug;
    }
    public ScrewInThePlug(UartService uartService, ServoMotorService servoMotorService,StepperMotorService stepperMotorService,
     SolenoidActuatorService solenoidActuatorService, LinearActuatorService linearActuatorService,string plugType)
    {
        PlugType = plugType;
        StepType = StepType.ScrewInThePlug;

        _uartService = uartService;
        _servoMotorService = servoMotorService;
        _stepperMotorService = stepperMotorService;
        _solenoidActuatorService = solenoidActuatorService;
        _linearActuatorService = linearActuatorService;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();

        await MainSequence();

        CompleteStep(true, "Plug has been plugged.");
    }

    private async Task MainSequence()
    {
         // Reset the rail
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.Clockwise, 4000);
        await _uartService!.AddDelayToBufferAsync(4);

        // Deatach tool
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 17);
        await _uartService!.AddDelayToBufferAsync(5);
        // Move to second bit
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 222);
        await _uartService!.AddDelayToBufferAsync(5);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.Clockwise, 12);
        await _uartService!.AddDelayToBufferAsync(5);

        // Move to plug dispenser
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 620);
        await _uartService!.AddDelayToBufferAsync(7);
        await _solenoidActuatorService!.MoveAsync(1, SolenoidActuatorService.ActuatorMovement.Extend);
        await _solenoidActuatorService!.MoveAsync(1, SolenoidActuatorService.ActuatorMovement.Retract);
        await _servoMotorService!.MoveAsync(100);
        await _uartService!.AddDelayToBufferAsync(3);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 12);
        await _servoMotorService!.MoveAsync(180);

        // Move to glue dispenser
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise,798);
        await _uartService!.AddDelayToBufferAsync(5);
        // Dispense glue
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 15);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 15);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 15);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 15);

        // Move to UR position
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 4000);
        
        _linearActuatorService!.SetPosition(0);
        await _uartService!.SendCommandBufferAsync();
        
        await _uartService!.WaitForStringAsync(">>>>INTERUPT");
        _linearActuatorService!.SetPosition(100);
        await _uartService!.WaitForStringAsync(">>>>Stepper 3 moved 17 steps<<<<");
        _linearActuatorService!.SetPosition(0);
        await _uartService!.WaitForStringAsync(">>>>Stepper 1 moved 222 steps<<<<");
        _linearActuatorService!.SetPosition(100);
        await _uartService!.WaitForStringAsync(">>>>Stepper 3 moved 12 steps<<<<");
        _linearActuatorService!.SetPosition(0);
        await _uartService!.WaitForStringAsync(">>>>Stepper 1 moved 620 steps<<<<");
        _linearActuatorService!.SetPosition(60);
        await _uartService!.WaitForStringAsync(">>>>ACTUATOR EXTENDED<<<<");
        Task.Delay(500).Wait();
        _linearActuatorService!.SetPosition(40);
        await _uartService!.WaitForStringAsync(">>>>Servo 1 moved to 100<<<<");
        _linearActuatorService!.SetPosition(60);
        await _uartService!.WaitForStringAsync(">>>>Servo 1 moved to 180<<<<");
        _linearActuatorService!.SetPosition(0);
        await _uartService!.WaitForStringAsync(">>>>Stepper 1 moved 798 steps<<<<");
        _linearActuatorService!.SetPosition(40);
        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 15 steps<<<<");
        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 15 steps<<<<");
        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 15 steps<<<<");
        await _uartService!.WaitForStringAsync(">>>>Stepper 2 moved 15 steps<<<<");
        await _uartService!.WaitForStringAsync(">>>>INTERUPT");
        _linearActuatorService!.SetPosition(100);
    }
}