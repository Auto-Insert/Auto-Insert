using AutoInsert.Core.Services.Control;
using AutoInsert.Core.Services.Control.StepHandlers;
using AutoInsert.Shared.Models;

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

            // Create a new sequence
            Console.WriteLine("Creating new sequence...");
            sequenceService.CreateNewSequence("Test Assembly", "Test sequence with multiple steps");
            Console.WriteLine("✓ Sequence created\n");

            // Get dependencies
            var urController = sequenceService.GetURController();
            var coordinateService = sequenceService.GetCoordinateService();
            var screwingController = sequenceService.GetScrewingStationController();

            if (urController == null || coordinateService == null || screwingController == null)
            {
                Console.WriteLine("✗ Failed to get dependencies");
                return;
            }

            // Create test waypoints
            var waypoint1 = new LocalWaypoint("Home", 0, 0, -100);
            var waypoint2 = new LocalWaypoint("Target",  452.6, 61.6, 17);
            var waypoint3 = new LocalWaypoint("Target lift",  452.6, 61.6, -100);

            // Add steps to the sequence
            Console.WriteLine("Adding steps to sequence:");
            
            var step1 = new MoveURToPositionStep
            {
                Name = "Move to target",
                Description = "Move robot to (0, 0, 100)",
                StepType = StepType.MoveURToPosition,
                TargetPosition = waypoint1,
                Speed = 0.1,
                Acceleration = 0.1,
                GripPart = false
            };
            sequenceService.GetCurrentSequence()?.Steps.Add(step1);
            Console.WriteLine("  ✓ Step 1: Move to target (100, 0, 0)");

            var step2 = new MoveURToPositionStep
            {
                Name = "Move Above Part",
                Description = "Move robot to part",
                StepType = StepType.MoveURToPosition,
                TargetPosition = waypoint3,
                Speed = 2,
                Acceleration = 1.5,
                GripPart = false
            };
            sequenceService.GetCurrentSequence()?.Steps.Add(step2);
            var step3 = new MoveURToPositionStep
            {
                Name = "Move to Part",
                Description = "Move robot to part",
                StepType = StepType.MoveURToPosition,
                TargetPosition = waypoint2,
                Speed = 2,
                Acceleration = 1.5,
                GripPart = true
            };
            sequenceService.GetCurrentSequence()?.Steps.Add(step3);
            Console.WriteLine("  ✓ Step 2: Lift off with tool");
            var step4 = new MoveURToPositionStep
            {
                Name = "Grip and lift",
                Description = "Grip and lift block",
                StepType = StepType.MoveURToPosition,
                TargetPosition = waypoint3,
                Speed = 2,
                Acceleration = 1.5,
                GripPart = true
            };
            sequenceService.GetCurrentSequence()?.Steps.Add(step4);
            var step5 = new MoveURToPositionStep
            {
                Name = "Go back",
                Description = "Release the gripped part at home position",
                StepType = StepType.MoveURToPosition,
                TargetPosition = waypoint2,
                Speed = 2,
                Acceleration = 1.5,
                GripPart = true
            };
            sequenceService.GetCurrentSequence()?.Steps.Add(step5);
            var step6 = new MoveURToPositionStep
            {
                Name = "Release it",
                Description = "Release the gripped part at home position",
                StepType = StepType.MoveURToPosition,
                TargetPosition = waypoint2,
                Speed = 2,
                Acceleration = 1.5,
                GripPart = false
            };
            sequenceService.GetCurrentSequence()?.Steps.Add(step6);
            var step7 = new MoveURToPositionStep
            {
                Name = "Go back home",
                Description = "Release the gripped part at home position",
                StepType = StepType.MoveURToPosition,
                TargetPosition = waypoint1,
                Speed = 2,
                Acceleration = 1.5,
                GripPart = false
            };
            sequenceService.GetCurrentSequence()?.Steps.Add(step7);
            Console.WriteLine("  ✓ Step 3: Release the tool\n");

            // Display sequence info
            var currentSequence = sequenceService.GetCurrentSequence();
            Console.WriteLine($"Sequence: {currentSequence?.Name}");
            Console.WriteLine($"Description: {currentSequence?.Description}");
            Console.WriteLine($"Total steps: {currentSequence?.Steps.Count}\n");

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
                Console.WriteLine($"✗ Failed: {args.step.Name} - {args.errorMessage}");
            };

            bool executed = await sequenceService.ExecuteSequenceAsync();
            
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
