using AutoInsert.Shared.Models;
using AutoInsert.Core.Controllers;

namespace AutoInsert.Core;  
internal class Program
{
    async static Task Main(string[] args)
    {
        UR ur = new UR("192.168.1.1");
        URController urController = new(ur);

        // Connect to robot
        await urController.ConnectAsync();
        
        await urController.EnableFreedriveAsync();
    
    }
}