using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Communication;

public class LinearActuatorService
{
    private readonly string _executablePath;

    public LinearActuatorService(string? executablePath = null)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _executablePath = Path.Combine(appDirectory, "lac_control.exe");
        }
        else
        {
            _executablePath = executablePath;
        }
    }
    public ExecutableStatus SetPosition(int percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            return new ExecutableStatus(false, "Percentage must be between 0 and 100.", -1);
        }
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _executablePath,
                Arguments = percentage.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                return new ExecutableStatus(false, "Failed to start the linear actuator control process.", -1);
            }
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Error: {error}");
                return new ExecutableStatus(false, error, process.ExitCode);
            }
            return new ExecutableStatus(true, output, process.ExitCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while executing linear actuator control: {ex.Message}");
            return new ExecutableStatus(false, ex.Message, -1);
        }
    }
}
