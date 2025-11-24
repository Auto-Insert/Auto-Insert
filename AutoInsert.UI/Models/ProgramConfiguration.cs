namespace AutoInsert.UI;

public class ProgramConfiguration(string programPath, int blockCount)
{
    public string ProgramPath { get; set; } = programPath;
    public int BlockCount { get; set; } = blockCount;
}