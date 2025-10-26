using AutoInsert.Core.Services;

namespace AutoInsert.Core;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide the file path as an argument.");
            return;
        }

        // Read plugs from filePath provided as the first argument
        var plugs = ThreadHoleReadingService.ReadThreadHolesFromFile(args[0]);

        foreach (var plug in plugs)
        {
            Console.WriteLine($"Type: {plug.PlugType}, Position: ({plug.PositionX}, {plug.PositionY}, {plug.PositionZ}), Vector: ({plug.VectorX}, {plug.VectorY}, {plug.VectorZ})");
        }
    }
}