namespace AutoInsert.Shared.Models;

public class ExecutableStatus(bool success, string output, int exitCode)
{
    public bool Success { get; set; } = success;
    public string Output { get; set; } = output;
    public int ExitCode { get; set; } = exitCode;
}