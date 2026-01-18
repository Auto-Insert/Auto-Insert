using AutoInsert.Shared.Models;
using AutoInsert.Core.Controllers;
namespace AutoInsert.Core.Services.Control.StepHandlers;
public class RotatePartToThreadHole : SequenceStep
{
    private readonly URController? _urController;
    public CartesianPositions? ScrewingPosition { get; set; }
    public double Speed { get; set; } = 0.25;
    public double Acceleration { get; set; } = 0.1;
    public RotatePartToThreadHole()
    {
        StepType = StepType.RotatePartToThreadHole;
    }
    public RotatePartToThreadHole(URController urController, CoordinateService coordinateService, CartesianPositions  screwingPosition, double speed = 0.25, double acceleration = 0.1)
    {
        _urController = urController;
        StepType = StepType.RotatePartToThreadHole;
        ScrewingPosition = screwingPosition;
        Speed = speed;
        Acceleration = acceleration;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();

            string moveScript = $@"
def rotate():
    set_digital_out(0, True)
    target_pose = p[{ScrewingPosition!.X:F6}, {ScrewingPosition!.Y:F6}, {ScrewingPosition!.Z:F6}, {ScrewingPosition!.Rx:F6}, {ScrewingPosition!.Ry:F6}, {ScrewingPosition!.Rz:F6}]
    movel(target_pose, a={Acceleration:F2}, v={Speed:F2})
end
";

        bool success = await _urController!.SendURScriptAsync(moveScript);

        if (!success)
        {
            CompleteStep(false, "Failed to send URScript to rotate part to thread hole.");
            return;
        }
        // Get current position
        var positionsBeforeMovement = await _urController.GetCurrentCartesianPositionsAsync();

        // Send command to robot
        success = await _urController.SendURScriptAsync(moveScript);
        
        if (!success)
        {
            CompleteStep(false, "Failed to send move script to UR robot.");
            return;
        }

        // Wait for movement to complete by polling current position and verifying it
        bool positionReached = false;
        int maxChecks = 100;
        int checkIntervalMs = 100;
        int maxRetries = 3;
        int retryCount = 0;
        CartesianPositions? lastReadPosition = positionsBeforeMovement;
        int samePositionCount = 0;
        const int maxSamePositionChecks = 5; // If position hasn't changed after 5 checks, retry
        
        for (int i = 0; i < maxChecks; i++)
        {
            await Task.Delay(checkIntervalMs);
            var currentPosition = await _urController.GetCurrentCartesianPositionsAsync();
            
            if (currentPosition == null)
                continue;
            
            // Check if robot reached target position
            if (Math.Abs(currentPosition.X - ScrewingPosition!.X) < 0.001 &&
                Math.Abs(currentPosition.Y - ScrewingPosition!.Y) < 0.001 &&
                Math.Abs(currentPosition.Z - ScrewingPosition!.Z) < 0.001)
            {
                positionReached = true;
                break;
            }
            
            // Check if position hasn't changed (robot stuck or not moving)
            if (lastReadPosition != null &&
                Math.Abs(currentPosition.X - lastReadPosition.X) < 0.0001 &&
                Math.Abs(currentPosition.Y - lastReadPosition.Y) < 0.0001 &&
                Math.Abs(currentPosition.Z - lastReadPosition.Z) < 0.0001)
            {
                samePositionCount++;
                
                // If position hasn't changed after several checks, retry the command
                if (samePositionCount >= maxSamePositionChecks && retryCount < maxRetries)
                {
                    retryCount++;
                    samePositionCount = 0;
                    await _urController.SendURScriptAsync(moveScript);   
                    i = 0;
                }
            }
            else
            {
                // Position changed, reset counter
                samePositionCount = 0;
            }
            
            lastReadPosition = currentPosition;
        }
            
        CompleteStep(positionReached, positionReached ? null : $"UR robot did not reach the target position in time. Retries: {retryCount}");
    
    }
}