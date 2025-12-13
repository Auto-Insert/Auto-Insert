using Autoinsert.Shared.Models;

namespace AutoInsert.Shared.Models;

public class AppConfiguration
{
    // Connection settings
    public string? URRobotIpAddress { get; set; }
    public string? SerialPort { get; set; }
    public int? SerialBaudRate { get; set; }
    
    // Calibration data
    public CalibrationData? CalibrationData { get; set; }
    public DateTime? LastCalibrationTime { get; set; }
    public bool IsCalibrated { get; set; }
}
