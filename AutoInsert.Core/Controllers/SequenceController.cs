using AutoInsert.Core.Services.Control;
using AutoInsert.Core.Services.Control.StepHandlers;
using AutoInsert.Core.Services.Communication;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Controllers;

public class SequenceController
{
    private readonly SequenceService _sequenceService;

    // Events forwarded from the service
    public event EventHandler<SequenceStep>? StepStarted;
    public event EventHandler<SequenceStep>? StepCompleted;
    public event EventHandler<SequenceStep>? StepFailed;
    public event EventHandler<Sequence>? SequenceCompleted;
    public event EventHandler<Sequence>? SequenceFailed;

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
    public async Task<bool> ExecuteSequenceAsync(Sequence sequence)
    {
        return await _sequenceService.ExecuteSequenceAsync(sequence);
    }

    public async Task<Sequence> LoadHardcodedSequence()
    {
        var sequence = new Sequence();
        sequence.Name = "Demonstration Sequence";
        sequence.Description = "This demonstration sequence picks up a a part and moves it to the screwing position. Changes the tool bit, picks up the plug, applies Loctite, and simulates screwing it in. ";

        // Add steps to the sequence
        var resetRail = new ResetRail
        {
            Name = "Reset Rail Position",
            Description = "Move the rail to the start position.",
            Direction = StepperMotorService.Direction.Clockwise,
        };

        var detachTool = new DetachTool
        {
            Name = "Deattach Tool",
            Description = "Dettach current tool bit.",
        };
        
        var changeToolBit = new ChangeToolBit
        {
            Name = "Change tool bit.",
            Description = "Move to second tool bit position.",
        };

        var attachPlug = new AttachPlug
        {
            Name = "Attach plug",
            Description = "Attach the plug to the tool bit.",
        };

        var dispenseGlue = new DispenseGlue
        {
            Name = "Apply Loctite",
            Description = "Dispense glue on the plug.",
        };

        var screwPlug = new ScrewPlug
        {
            Name = "Screw in the plug",
            Description = "Simulate screwing the plug in to the part.",
        };

        var overPartPos = new LocalWaypoint("Over Part Position", 512.6, 141.6, -200.0);
        var moveUrOverPart = new MoveURToPosition
        {
            Name = "Move Robot above part",
            Description = "Move the UR robot to position above the part.",
            Speed = 0.1,
            Acceleration = 1.0,
            TargetPosition = overPartPos,
            GripPart = false
        };

        var partPos = new LocalWaypoint("Part Position", 512.6, 141.6, 15.0);
        var moveUrToPart = new MoveURToPosition
        {
            Name = "Move Robot to part",
            Description = "Move the UR robot to the part.",
            Speed = 0.1,
            Acceleration = 1.0,
            TargetPosition = partPos,
            GripPart = false
        };

        var gripAndLift = new MoveURToPosition
        {
            Name = "Lift Part",
            Description = "Grip the part and lift it.",
            Speed = 0.1,
            Acceleration = 1.0,
            TargetPosition = overPartPos,
            GripPart = true 
        };

        var screwingPos = new LocalWaypoint("Screwing Position", 255.2, 30, -250.0);
        var goToScrewingPosition = new MoveURToPosition
        {
            Name = "Move to screwing position",
            Description = "Move part to screwing position.",
            Speed = 0.1,
            Acceleration = 1.0,
            TargetPosition = screwingPos,
            GripPart = true 
        };


        var gripAndPutDown = new MoveURToPosition
        {
            Name = "Move Part back to holder",
            Description = "Move part back to the holder while gripping it.",
            Speed = 0.1,
            Acceleration = 0.5,
            TargetPosition = partPos,
            GripPart = true 
        };

        var moveBackFromScrewing = new MoveURToPosition
        {
            Name = "Return to holder",
            Description = "Move UR robot back to the holder after screwing.",
            Speed = 0.1,
            Acceleration = 1.0,
            TargetPosition = overPartPos,
            GripPart = true 
        };

        var goBackToHolder = new MoveURToPosition
        {
            Name = "Move Robot above holder",
            Description = "Move the UR robot to position above the part.",
            Speed = 0.1,
            Acceleration = 1.0,
            TargetPosition = overPartPos,
            GripPart = false
        };

        // Picking up part with UR
        sequence.Steps.Add(moveUrOverPart);
        sequence.Steps.Add(moveUrToPart);
        sequence.Steps.Add(gripAndLift);
        sequence.Steps.Add(goToScrewingPosition);

        // Sequence
        sequence.Steps.Add(resetRail);
        sequence.Steps.Add(detachTool);
        sequence.Steps.Add(changeToolBit);
        sequence.Steps.Add(attachPlug);
        sequence.Steps.Add(dispenseGlue);
        sequence.Steps.Add(screwPlug);

        // Moving back the part
        sequence.Steps.Add(resetRail);
        sequence.Steps.Add(moveBackFromScrewing);
        sequence.Steps.Add(gripAndPutDown);
        sequence.Steps.Add(goBackToHolder);

        return sequence;
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
    public List<string> GetSequenceNames()
    {
        return _sequenceService.GetSavedSequenceNames();
    }
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
