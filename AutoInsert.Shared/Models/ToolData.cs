namespace AutoInsert.Shared.Models;

public class ToolData
{
    public byte AnalogInputRange0 { get; set; }
    public byte AnalogInputRange1 { get; set; }
    public double AnalogInput0 { get; set; }
    public double AnalogInput1 { get; set; }
    public float ToolVoltage48V { get; set; }
    public byte ToolOutputVoltage { get; set; }
    public float ToolCurrent { get; set; }
    public float ToolTemperature { get; set; }
    public int ToolMode { get; set; }
}