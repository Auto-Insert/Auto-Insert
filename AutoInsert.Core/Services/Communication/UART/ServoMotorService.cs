using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Communication
{
    public class ServoMotorService
    {
        private readonly UartService _uartService;
        private const int COMMAND_LENGTH = 16;
        
        public ServoMotorService(UartService uartService)
        {
            _uartService = uartService ?? throw new ArgumentNullException(nameof(uartService));
        }
        public async Task<MoveStatus> MoveAsync(int degrees)
        {   
            string command = BuildCommand(degrees);
            try
            {
                bool response = await _uartService.SendCommandAsync(command);
                return new MoveStatus(response, $"Servo motor move {degrees} degree(s) command sent.");
            }
            catch (Exception ex)
            {   
                return new MoveStatus(false, $"Error sending command: {ex.Message}");  
            }
        }

        private string BuildCommand(int degrees)
        {
            // Format: NS1C1Dddd
            string command = $"NS1C1D{degrees}";
            
            // Fill rest of the command with X up to COMMAND_LENGTH
            return command.PadRight(COMMAND_LENGTH, 'X');
        }
    }
}
