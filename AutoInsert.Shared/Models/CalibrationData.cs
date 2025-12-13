using AutoInsert.Shared.Models;

namespace Autoinsert.Shared.Models;

public class CalibrationData(Waypoint zeroPoint, Waypoint xAxisPoint, Waypoint yAxisPoint)
{
    public Waypoint ZeroPoint { get; set; } = zeroPoint;
    public Waypoint XAxisPoint { get; set; } = xAxisPoint;
    public Waypoint YAxisPoint { get; set; } = yAxisPoint;
}