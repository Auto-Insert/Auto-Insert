namespace AutoInsert.Shared.Interfaces
{
    public interface IThreadHole
    {
        string PlugType { get; set; }
        double PositionX { get; set; }
        double PositionY { get; set; }
        double PositionZ { get; set; }
        double VectorX { get; set; }
        double VectorY { get; set; }
        double VectorZ { get; set; }
    }
}