using AutoInsert.Core.Controllers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;

public class AttachPlug : SequenceStep
{
    private readonly UartService? _uartService;
    private readonly StepperMotorService? _stepperMotorService;
    private readonly LinearActuatorService? _linearActuatorService;
    private readonly SolenoidActuatorService? _solenoidActuatorService;
    private readonly ServoMotorService? _servoMotorService;
    public StepperMotorService.Direction Direction { get; set; }
    public AttachPlug()
    {
        StepType = StepType.AttachPlug;
    }
    public AttachPlug(UartService uartService, StepperMotorService stepperMotorService, LinearActuatorService linearActuatorService,
    SolenoidActuatorService solenoidActuatorService, ServoMotorService servoMotorService)
    {
        StepType = StepType.AttachPlug;

        _uartService = uartService;
        _stepperMotorService = stepperMotorService;
        _linearActuatorService = linearActuatorService;
        _solenoidActuatorService = solenoidActuatorService;
        _servoMotorService = servoMotorService;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();
        // Move to plug dispenser
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 620);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Stepper 1 moved 620 steps<<<<");
        _linearActuatorService!.SetPosition(60);
        Task.Delay(1000).Wait();

        // Drop Plug and pick it up.
        await _solenoidActuatorService!.MoveAsync(2, SolenoidActuatorService.ActuatorMovement.Extend);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>ACTUATOR EXTENDED<<<<");
        Task.Delay(500).Wait();
        _linearActuatorService!.SetPosition(40);

        // Open servo to grab the plug
        await _servoMotorService!.MoveAsync(120);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Servo 1 moved to 120<<<<");
        _linearActuatorService!.SetPosition(65);

        // Close servo to secure the plug
        await _stepperMotorService!.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 12);
        await _servoMotorService!.MoveAsync(180);
        await _uartService!.SendCommandBufferAsync();
        await _uartService!.WaitForStringAsync(">>>>Servo 1 moved to 180<<<<");
        _linearActuatorService!.SetPosition(0);

        // Cleanup
        await _solenoidActuatorService!.MoveAsync(2, SolenoidActuatorService.ActuatorMovement.Retract);
        await _uartService!.SendCommandBufferAsync();

        Task.Delay(2000).Wait();

        CompleteStep(true, "Plug attached.");
    }
}