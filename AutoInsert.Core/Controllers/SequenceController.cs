using AutoInsert.Core.Services.Control;
using AutoInsert.Core.Services.Data;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Controllers;

public class SequenceController
{
    private readonly SequenceService _sequenceService;

    // Events forwarded from the service
    public event EventHandler<SequenceStep>? StepStarted;
    public event EventHandler<SequenceStep>? StepCompleted;
    public event EventHandler<(SequenceStep step, string errorMessage)>? StepFailed;
    public event EventHandler<Sequence>? SequenceCompleted;
    public event EventHandler<(Sequence Sequence, string Error)>? SequenceFailed;

    public SequenceController()
    {
        _sequenceService = new SequenceService();
        
        // Forward events from service to controller
        _sequenceService.StepStarted += (s, e) => StepStarted?.Invoke(this, e);
        _sequenceService.StepCompleted += (s, e) => StepCompleted?.Invoke(this, e);
        _sequenceService.StepFailed += (s, e) => StepFailed?.Invoke(this, e);
        _sequenceService.SequenceCompleted += (s, e) => SequenceCompleted?.Invoke(this, e);
        _sequenceService.SequenceFailed += (s, e) => SequenceFailed?.Invoke(this, e);
    }

    // Initialization
    public async Task InitializeAsync()
    {
        await _sequenceService.InitializeAsync();
    }

    // Sequence management
    public void CreateNewSequence(string name, string? description = null)
    {
        _sequenceService.CreateNewSequence(name, description);
    }

    public void LoadSequence(Sequence sequence)
    {
        _sequenceService.LoadSequence(sequence);
    }

    public Sequence? GetCurrentSequence()
    {
        return _sequenceService.GetCurrentSequence();
    }

    // Step management
    public void AddStep(SequenceStep step)
    {
        var currentSequence = _sequenceService.GetCurrentSequence();
        if (currentSequence == null)
        {
            throw new InvalidOperationException("No sequence loaded. Create a new sequence first.");
        }
        
        currentSequence.Steps.Add(step);
    }

    public void RemoveStep(SequenceStep step)
    {
        var currentSequence = _sequenceService.GetCurrentSequence();
        currentSequence?.Steps.Remove(step);
    }

    public void MoveStepUp(SequenceStep step)
    {
        var currentSequence = _sequenceService.GetCurrentSequence();
        if (currentSequence == null) return;

        int index = currentSequence.Steps.IndexOf(step);
        if (index > 0)
        {
            currentSequence.Steps.RemoveAt(index);
            currentSequence.Steps.Insert(index - 1, step);
        }
    }

    public void MoveStepDown(SequenceStep step)
    {
        var currentSequence = _sequenceService.GetCurrentSequence();
        if (currentSequence == null) return;

        int index = currentSequence.Steps.IndexOf(step);
        if (index >= 0 && index < currentSequence.Steps.Count - 1)
        {
            currentSequence.Steps.RemoveAt(index);
            currentSequence.Steps.Insert(index + 1, step);
        }
    }

    // Execution
    public async Task<bool> ExecuteSequenceAsync()
    {
        return await _sequenceService.ExecuteSequenceAsync();
    }

    public void CancelSequence()
    {
        _sequenceService.CancelSequence();
    }

    // Storage operations
    public async Task<bool> SaveCurrentSequenceAsync()
    {
        return await _sequenceService.SaveCurrentSequenceAsync();
    }

    public async Task<Sequence?> LoadSequenceFromStorageAsync(string sequenceName)
    {
        return await _sequenceService.LoadSequenceFromStorageAsync(sequenceName);
    }

    public async Task<List<Sequence>> LoadAllSequencesAsync()
    {
        return await _sequenceService.LoadAllSequencesAsync();
    }

    public bool DeleteSequence(string sequenceName)
    {
        return _sequenceService.DeleteSequence(sequenceName);
    }

    // Helper methods for frontend
    public List<StepType> GetAvailableStepTypes()
    {
        return Enum.GetValues(typeof(StepType)).Cast<StepType>().ToList();
    }
    public SequenceStep CreateStep(StepType stepType)
    {
        return stepType switch
        {
            StepType.MoveURToPosition => new MoveURToPositionStep
            {
                Name = "Move Robot",
                Description = "Move robot to target position",
                StepType = StepType.MoveURToPosition,
                Speed = 0.5,
                Acceleration = 1.0,
                GripPart = false
            },
            StepType.SetScrewdriverExtension => new SetScrewdriverExtensionStep
            {
                Name = "Set Screwdriver",
                Description = "Set screwdriver extension percentage",
                StepType = StepType.SetScrewdriverExtension,
                Percentage = 0
            },
            _ => throw new ArgumentException($"Unknown step type: {stepType}")
        };
    }
}
