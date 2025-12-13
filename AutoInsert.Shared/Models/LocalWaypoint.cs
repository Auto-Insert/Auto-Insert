namespace AutoInsert.Shared.Models;

public class LocalWaypoint
{
    public string Name { get; set; } = string.Empty;
    public double X { get; set; } // in millimeters
    public double Y { get; set; } // in millimeters
    public double Z { get; set; } // in millimeters
    
    public LocalWaypoint()
    {
    }
    
    public LocalWaypoint(string name, double x, double y, double z)
    {
        Name = name;
        X = x;
        Y = y;
        Z = z;
    }
}
