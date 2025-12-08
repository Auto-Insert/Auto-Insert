namespace AutoInsert.Shared.Models;

public class Waypoint
{
    public string? Name { get; set; }
    public double[]? JointPositions { get; set; } = new double[6];
    public CartesianPositions? CartesianPositions { get; set; }
}