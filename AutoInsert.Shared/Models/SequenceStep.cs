using System.Text.Json.Serialization;

namespace AutoInsert.Shared.Models;

// Base step class
[JsonDerivedType(typeof(MoveURToPositionStep), typeDiscriminator: "moveUR")]
[JsonDerivedType(typeof(SetScrewdriverExtensionStep), typeDiscriminator: "setScrewdriver")]
public abstract class SequenceStep
{
    public string Name { get; set; } = string.Empty;
    public StepType StepType { get; set; }
    public string? Description { get; set; }
    
    // Execution tracking - excluded from serialization
    [JsonIgnore]
    public StepStatus Status { get; set; } = StepStatus.NotStarted;
    
    [JsonIgnore]
    public bool IsCompleted => Status == StepStatus.Completed;
    
    [JsonIgnore]
    public bool HasError => Status == StepStatus.Failed;
    
    [JsonIgnore]
    public DateTime? StartTime { get; set; }
    
    [JsonIgnore]
    public DateTime? EndTime { get; set; }
    
    [JsonIgnore]
    public string? ErrorMessage { get; set; }
    
    [JsonIgnore]
    public TimeSpan? Duration => StartTime.HasValue && EndTime.HasValue 
        ? EndTime.Value - StartTime.Value 
        : null;
    
    protected void StartStep()
    {
        StartTime = DateTime.Now;
        Status = StepStatus.Running;
    }
    
    protected void CompleteStep(bool success, string? errorMessage = null)
    {
        EndTime = DateTime.Now;
        Status = success ? StepStatus.Completed : StepStatus.Failed;
        if (!success)
        {
            ErrorMessage = errorMessage;
        }
    }
    
    public abstract Task ExecuteAsync();
}

// Serializable step data classes
public class MoveURToPositionStep : SequenceStep
{
    public LocalWaypoint? TargetPosition { get; set; }
    public double Speed { get; set; } = 1.0;
    public double Acceleration { get; set; } = 1.0;
    public bool GripPart { get; set; }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException("Use MoveURToPosition handler class for execution");
    }
}

public class SetScrewdriverExtensionStep : SequenceStep
{
    public int Percentage { get; set; }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException("Use SetScrewdriverExtension handler class for execution");
    }
}