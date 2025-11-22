namespace AutoInsert.UI;

public class ProcessResult
{
    public bool Success { get; set; }
    public int BlocksProcessed { get; set; }
    public TimeSpan Duration { get; set; }
    public string ErrorMessage { get; set; }
}