using AutoInsert.Shared.Models;
using AutoInsert.Core.Controllers;
namespace AutoInsert.Core.Services.Control.StepHandlers;
public class RotatePartToThreadHole : SequenceStep
{
    private readonly URController? _urController;
    public ThreadHole? TargetHole { get; set; }
    public Waypoint? ScrewingStation { get; set; }
    public RotatePartToThreadHole()
    {
        StepType = StepType.RotatePartToThreadHole;
    }
    public RotatePartToThreadHole(URController urController, CoordinateService coordinateService, ThreadHole hole, Waypoint? screwingStation)
    {
        _urController = urController;
        StepType = StepType.RotatePartToThreadHole;
        TargetHole = hole;
        ScrewingStation = screwingStation;
    }
    public override async Task ExecuteAsync()
    {
        StartStep();

        string urScript = $@"";

        bool success = await _urController!.SendURScriptAsync(urScript);

        if (!success)
        {
            CompleteStep(false, "Failed to send URScript to rotate part to thread hole.");
            return;
        }
    
    }
}