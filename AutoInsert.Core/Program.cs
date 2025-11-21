using AutoInsert.Shared.Models;
using AutoInsert.Core.Controllers;
using System.Globalization;

namespace AutoInsert.Core;  
internal class Program
{
    async static Task Main(string[] args)
    {
        UR ur = new UR("192.168.1.1");
        URController urController = new(ur);

        // Connect to robot
        await urController.ConnectAsync();
        
        string robotmode = await urController.GetRobotModeAsync();
        Console.WriteLine($"Robot Mode: {robotmode}\n");

        // Get current joint positions
        Waypoint? waypoint = await urController.GetCurrentPositionAsync();
        if (waypoint != null)
        {
            Console.WriteLine("Current Joint Positions:");
            for (int i = 0; i < waypoint.JointPositions.Length; i++)
            {
                Console.WriteLine($"  Joint {i + 1}: {waypoint.JointPositions[i].ToString("F4", CultureInfo.InvariantCulture)} rad");
            }
        }
    
    }
}