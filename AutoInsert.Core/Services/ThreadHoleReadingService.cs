using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services
{
    public class ThreadHoleReadingService
    {   
        public static List<ThreadHole> ReadThreadHolesFromFile(string filePath, string seperator = ";")
        {
            var threadHoles = new List<ThreadHole>();

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(seperator);
                if (parts.Length == 7)
                {
                    var threadHole = new ThreadHole(
                        parts[0],
                        double.Parse(parts[1]),
                        double.Parse(parts[2]),
                        double.Parse(parts[3]),
                        double.Parse(parts[4]),
                        double.Parse(parts[5]),
                        double.Parse(parts[6])
                    );
                    threadHoles.Add(threadHole);
                }
            }
            return threadHoles;
        }
    }
}