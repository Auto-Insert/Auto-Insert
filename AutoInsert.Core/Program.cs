using AutoInsert.Shared.Models;
using AutoInsert.Core.Controllers;

namespace AutoInsert.Core;  
internal class Program
{
    async static Task Main(string[] args)
    {
        UR ur = new UR("192.168.0.108");
        URController urController = new(ur);

        // Connect to robot
        await urController.ConnectAsync();
        
        // Get current joint positions
        var jointPositions = await urController.GetCurrentJointPositionsAsync();
        if (jointPositions != null)
        {
            Console.WriteLine("Current Joint Positions:");
            Console.WriteLine(string.Join(", ", jointPositions.Select(p => p.ToString("F4"))));
        }
        else
        {
            Console.WriteLine("Failed to retrieve joint positions.");
        }
        var cartesianPositions = await urController.GetCurrentCartesianPositionsAsync();
        if (cartesianPositions != null)
        {
            Console.WriteLine("Current Cartesian Positions:");
            Console.WriteLine($"X: {cartesianPositions.X:F4}, Y: {cartesianPositions.Y:F4}, Z: {cartesianPositions.Z:F4}, RX: {cartesianPositions.Rx:F4}, RY: {cartesianPositions.Ry:F4}, RZ: {cartesianPositions.Rz:F4}");
        }
        else
        {
            Console.WriteLine("Failed to retrieve cartesian positions.");
        }
        var waypoint = await urController.GetWaypointFromCurrentPositionsAsync("Home");
        if (waypoint != null)
        {
            Console.WriteLine("Waypoint 'Home':");
            Console.WriteLine("Joint Positions: " + string.Join(", ", waypoint.JointPositions!.Select(p => p.ToString("F4"))));
            if (waypoint.CartesianPositions != null)
            {
                Console.WriteLine($"Cartesian Positions: X: {waypoint.CartesianPositions.X:F4}, Y: {waypoint.CartesianPositions.Y:F4}, Z: {waypoint.CartesianPositions.Z:F4}, RX: {waypoint.CartesianPositions.Rx:F4}, RY: {waypoint.CartesianPositions.Ry:F4}, RZ: {waypoint.CartesianPositions.Rz:F4}, TCPOffsetX: {waypoint.CartesianPositions.TCPOffsetX:F4}, TCPOffsetY: {waypoint.CartesianPositions.TCPOffsetY:F4}, TCPOffsetZ: {waypoint.CartesianPositions.TCPOffsetZ:F4}");
            }
        }
        else
        {
            Console.WriteLine("Failed to retrieve waypoint.");
        }
    }
}