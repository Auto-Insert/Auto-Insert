using System.Globalization;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services
{
    public class ThreadHoleReadingService
    {
        private static readonly CultureInfo Culture = new("da-DK"); // Danish culture uses comma as decimal separator
        public static List<ThreadHole> ReadThreadHolesFromFile(string filePath, string separator = ";")
        {
            var threadHoles = new List<ThreadHole>();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(separator);
                if (parts.Length == 7)
                {
                    var threadHole = new ThreadHole(
                        parts[0],
                        double.Parse(parts[1], Culture),
                        double.Parse(parts[2], Culture),
                        double.Parse(parts[3], Culture),
                        double.Parse(parts[4], Culture),
                        double.Parse(parts[5], Culture),
                        double.Parse(parts[6], Culture)
                    );
                    threadHoles.Add(threadHole);
                }
            }
            return threadHoles;
        }
    }
}