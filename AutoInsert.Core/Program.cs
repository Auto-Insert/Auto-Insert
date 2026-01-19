using AutoInsert.Core.Services.Control;
using AutoInsert.Core.Services.Control.StepHandlers;
using AutoInsert.Shared.Models;
using AutoInsert.Core.Services.Communication;

namespace AutoInsert.Core;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== AutoInsert Sequence Service Test ===\n");

        try
        {
            // Initialize the sequence service
            var sequenceService = new SequenceService();
            Console.WriteLine("Initializing sequence service...");
            await sequenceService.InitializeAsync();
            Console.WriteLine("✓ Service initialized\n");

            var sequence = new Sequence();
            sequence.Name = "Test Sequence 1";
            sequence.Description = "Test sequence";

            // Add steps to the sequence
            var resetRail = new ResetRail
            {
                Name = "Reset Rail",
                Description = "Reset the rail to start position",
                Direction = StepperMotorService.Direction.Clockwise,
            };

            var detachTool = new DetachTool
            {
                Name = "Detach Tool",
                Description = "Detach current tool",
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
                Description = "Screw the plug in to the part.",
            };

            sequence.Steps.Add(resetRail);
            sequence.Steps.Add(detachTool);
            sequence.Steps.Add(changeToolBit);
            sequence.Steps.Add(attachPlug);
            sequence.Steps.Add(dispenseGlue);
            sequence.Steps.Add(screwPlug);


            // Execute the sequence
            Console.WriteLine("=== Executing Sequence ===\n");
            
            sequenceService.StepStarted += (sender, step) =>
            {
                Console.WriteLine($"▶ Starting: {step.Name}");
            };
            
            sequenceService.StepCompleted += (sender, step) =>
            {
                Console.WriteLine($"✓ Completed: {step.Name}");
            };
            
            sequenceService.StepFailed += (sender, args) =>
            {
                Console.WriteLine($"✗ Failed: {args.Name} - {args.ErrorMessage}");
            };

            bool executed = await sequenceService.ExecuteSequenceAsync(sequence);
            
            if (executed)
            {
                Console.WriteLine("\n✓ Sequence execution completed successfully");
            }
            else
            {
                Console.WriteLine("\n✗ Sequence execution failed");
            }

            Console.WriteLine("\n=== Test Complete ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}