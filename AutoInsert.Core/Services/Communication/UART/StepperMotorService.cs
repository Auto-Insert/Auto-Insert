using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Communication
{
    public class StepperMotorService
    {
        public readonly UartService UartService;
        private const int COMMAND_LENGTH = 15;
        
        public enum Motor
        {
            Rail = 1,
            Pump = 2,
            Tool = 3
        }
        
        public enum Direction
        {
            AntiClockwise = 0,
            Clockwise = 1
        }
        
        public StepperMotorService(UartService uartService)
        {
            UartService = uartService ?? throw new ArgumentNullException(nameof(uartService));
        }
        
        public async Task<MoveStatus> MoveAsync(Motor motor, Direction direction, int steps)
        {
            if (steps < 0 || steps > 30000)
                return new MoveStatus(false, "Steps must be between 0 and 30000.");
            
            string command = BuildCommand(motor, direction, steps);
            try
            {
                bool response = UartService.AddCommandToBuffer(command);
                if (!response)
                {
                    return new MoveStatus(false, "Failed to move stepper motor.");
                }
                return new MoveStatus(response, $"Stepper motor move {steps} step(s) command sent.");
            }
            catch (Exception ex)
            {
                return new MoveStatus(false, $"Error sending command: {ex.Message}");
            }
            
        }
        
        private string BuildCommand(Motor motor, Direction direction, int steps)
        {
            // Format: NMmC1DdSsssss
            string command = $"NM{(int)motor}C1D{(int)direction}S{steps}";

            // Fill rest of the command with X up to COMMAND_LENGTH
            return command.PadRight(COMMAND_LENGTH, 'X');
        }
    }       
}