using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Control.StepHandlers;
public class SetScrewdriverExtension : SequenceStep
{
    private readonly ScrewingStationController? _screwingStationController;
    public int ExtensionPercentage;
    public SetScrewdriverExtension()
    {
        StepType = StepType.SetScrewdriverExtension;
    }
    public SetScrewdriverExtension(ScrewingStationController screwingStationController, int extensionPercentage)
    {
        StepType = StepType.SetScrewdriverExtension;
        _screwingStationController = screwingStationController;
        ExtensionPercentage = extensionPercentage;
    }
    public override async Task ExecuteAsync()
    {
        try
        {
            StartStep();

            ExecutableStatus status = await Task.Run(() => _screwingStationController.ExtendScrewdriver(ExtensionPercentage));

            CompleteStep(status.Success, status.Success ? null : status.Output);
        }
        catch (Exception ex)
        {
            CompleteStep(false, ex.Message);
        }
    }
}