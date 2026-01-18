using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Communication
{
    public class SolenoidActuatorService
    {
        public readonly UartService _uartService;
        private const int COMMAND_LENGTH = 15;
        private const char ExtendCommand = 'E';
        private const char RetractCommand = 'R';
        public enum ActuatorMovement
        {
            Extend,
            Retract
        }
        public SolenoidActuatorService(UartService uartService)
        {
            _uartService = uartService ?? throw new ArgumentNullException(nameof(uartService));
        }
        public async Task<MoveStatus> MoveAsync(int actuator, ActuatorMovement movement)
        {   
            char movementChar = movement == ActuatorMovement.Extend ? ExtendCommand : RetractCommand;
            string command = BuildCommand(actuator, movementChar);
            try
            {
                bool response = _uartService.AddCommandToBuffer(command);
                if (!response)
                {
                    return new MoveStatus(false, "Failed to move solenoid actuator.");
                }
                return new MoveStatus(response, $"Solenoid actuator ({actuator}) {movement} command sent.");
            }
            catch (Exception ex)
            {   
                return new MoveStatus(false, $"Error sending command: {ex.Message}");  
            }
        }
        
        private string BuildCommand(int actuator, char movement)
        {
            // Format: NAaC1Dp
            string command = $"NA{actuator}C1D{movement}";
            
            // Fill rest of the command with X up to COMMAND_LENGTH
            return command.PadRight(COMMAND_LENGTH, 'X');
        }
    }       
}