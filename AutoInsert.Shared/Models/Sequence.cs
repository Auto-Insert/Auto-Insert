namespace AutoInsert.Shared.Models;

public class Sequence
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SequenceStep> Steps { get; set; } = new List<SequenceStep>();
}