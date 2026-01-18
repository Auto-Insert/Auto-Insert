using AutoInsert.Core.Services.Control;
using AutoInsert.Core.Services.Control.StepHandlers;
using AutoInsert.Shared.Models;
using AutoInsert.Core.Services.Communication;

namespace AutoInsert.Core;

class Program
{
    static async Task Main()
    {
        var uartService = new UartService();
        bool isConnected = uartService.Connect("COM8", 115200);
        if (!isConnected)
        {
            Console.WriteLine("Failed to connect to UART.");
            return;
        }
        var servoMotorService = new ServoMotorService(uartService);
        var stepperMotorService = new StepperMotorService(uartService);
        var solenoidActuatorService = new SolenoidActuatorService(uartService);
        var linearActuatorService = new LinearActuatorService();
        
        // Reset the rail
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.Clockwise, 4000);
        await uartService.AddDelayToBufferAsync(4);
        // Deatach tool
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.Clockwise, 25);
        await uartService.AddDelayToBufferAsync(1);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.Clockwise, 1000);
        await uartService.AddDelayToBufferAsync(5);
        // Move to second bit
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 222);
        await uartService.AddDelayToBufferAsync(2);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.Clockwise, 400);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 401);
        await uartService.AddDelayToBufferAsync(5);

        // Move to plug dispenser
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 620);
        await uartService.AddDelayToBufferAsync(7);
        await solenoidActuatorService.MoveAsync(1, SolenoidActuatorService.ActuatorMovement.Extend);
        await solenoidActuatorService.MoveAsync(1, SolenoidActuatorService.ActuatorMovement.Retract);
        await servoMotorService.MoveAsync(100);
        await uartService.AddDelayToBufferAsync(3);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 12);
        await servoMotorService.MoveAsync(180);

        // Move to glue dispenser
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise,798);
        await uartService.AddDelayToBufferAsync(5);
        // Dispense glue
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 50);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Pump, StepperMotorService.Direction.AntiClockwise, 100);
        // Move to UR position
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Rail, StepperMotorService.Direction.AntiClockwise, 4000);
        await uartService.AddDelayToBufferAsync(1);
        await stepperMotorService.MoveAsync(StepperMotorService.Motor.Tool, StepperMotorService.Direction.AntiClockwise, 400);
        
        linearActuatorService.SetPosition(0);
        Task.Delay(5000).Wait();
        await uartService.SendCommandBufferAsync();
        
        await uartService.WaitForStringAsync(">>>>INTERUPT");
        linearActuatorService.SetPosition(80);
        await uartService.WaitForStringAsync(">>>>Stepper 3 moved 25 steps<<<<");
        linearActuatorService.SetPosition(90);
        await uartService.WaitForStringAsync(">>>>Stepper 3 moved 1000 steps<<<<");
        linearActuatorService.SetPosition(0);
        await uartService.WaitForStringAsync(">>>>Stepper 1 moved 222 steps<<<<");
        linearActuatorService.SetPosition(100);
        await uartService.WaitForStringAsync(">>>>Stepper 3 moved 401 steps<<<<");
        linearActuatorService.SetPosition(0);
        await uartService.WaitForStringAsync(">>>>Stepper 1 moved 620 steps<<<<");
        linearActuatorService.SetPosition(60);
        await uartService.WaitForStringAsync(">>>>ACTUATOR EXTENDED<<<<");
        Task.Delay(500).Wait();
        linearActuatorService.SetPosition(40);
        await uartService.WaitForStringAsync(">>>>Servo 1 moved to 100<<<<");
        linearActuatorService.SetPosition(60);
        await uartService.WaitForStringAsync(">>>>Servo 1 moved to 180<<<<");
        linearActuatorService.SetPosition(0);
        await uartService.WaitForStringAsync(">>>>Stepper 1 moved 798 steps<<<<");
        linearActuatorService.SetPosition(20);
        await uartService.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");
        await uartService.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");
        await uartService.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");
        await uartService.WaitForStringAsync(">>>>Stepper 2 moved 100 steps<<<<");
        await uartService.WaitForStringAsync(">>>>INTERUPT");
        linearActuatorService.SetPosition(100);
    }
}
