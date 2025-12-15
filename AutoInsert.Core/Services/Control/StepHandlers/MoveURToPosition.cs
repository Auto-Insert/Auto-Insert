using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;
public class MoveURToPosition : SequenceStep
{
    private readonly URController? _urController;
    private readonly CoordinateService? _coordinateService;
    
    public LocalWaypoint? TargetPosition { get; set; }
    public double Speed { get; set; } = 1.0;
    public double Acceleration { get; set; } = 1.0;
    public bool GripPart { get; set; }
    private const int GripVoltage = 12;
    public MoveURToPosition()
    {
        StepType = StepType.MoveURToPosition;
    }
    public MoveURToPosition(URController urController, CoordinateService coordinateService, LocalWaypoint targetPosition, double speed, double acceleration, bool gripPart = false)
    {
        _urController = urController;
        _coordinateService = coordinateService;
        StepType = StepType.MoveURToPosition;
        TargetPosition = targetPosition;
        Speed = speed;
        Acceleration = acceleration;
        GripPart = gripPart;
    }
    public override async Task ExecuteAsync()
    {
        try
        {
            if (_urController == null || _coordinateService == null || TargetPosition == null)
            {
                CompleteStep(false, "Required dependencies or target position are not set.");
                return;
            }

            StartStep();
            
            // Convert local coordinates (mm) to meters
            double localXMeters = TargetPosition.X / 1000.0;
            double localYMeters = TargetPosition.Y / 1000.0;
            double localZMeters = TargetPosition.Z / 1000.0;
            
            // Convert to UR Robot coordinates
            var robotPosition = _coordinateService.LocalToGlobal(localXMeters, localYMeters, localZMeters);
            
            // Create URScript to move to position
            string gripValue = GripPart ? "True" : "False";
            string moveScript = $@"def move_ur_to_position():
    set_tool_voltage({(GripPart ? GripVoltage : 0)})
    set_digital_out(8, {gripValue})
    target_pose = p[{robotPosition.X:F6}, {robotPosition.Y:F6}, {robotPosition.Z:F6}, {robotPosition.Rx:F6}, {robotPosition.Ry:F6}, {robotPosition.Rz:F6}]
    movel(target_pose, a={Acceleration:F2}, v={Speed:F2})
end
";

            // Get current position
            var positionsBeforeMovement = await _urController.GetCurrentCartesianPositionsAsync();

            // Send command to robot
            bool success = await _urController.SendURScriptAsync(moveScript);
            
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
                if (Math.Abs(currentPosition.X - robotPosition.X) < 0.001 &&
                    Math.Abs(currentPosition.Y - robotPosition.Y) < 0.001 &&
                    Math.Abs(currentPosition.Z - robotPosition.Z) < 0.001)
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
        catch (Exception ex)
        {
            Status = StepStatus.Failed;
            ErrorMessage = ex.Message;
            EndTime = DateTime.Now;
        }
    }
}