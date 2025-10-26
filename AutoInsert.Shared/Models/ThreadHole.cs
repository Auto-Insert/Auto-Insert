namespace AutoInsert.Shared.Models
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

    public class ThreadHole(string plugType, double positionX, double positionY, double positionZ, double vectorX, double vectorY, double vectorZ) : IThreadHole
    {
        public string PlugType { get; set; } = plugType;
        public double PositionX { get; set; } = positionX;
        public double PositionY { get; set; } = positionY;
        public double PositionZ { get; set; } = positionZ;
        public double VectorX { get; set; } = vectorX;
        public double VectorY { get; set; } = vectorY;
        public double VectorZ { get; set; } = vectorZ;
    }
}