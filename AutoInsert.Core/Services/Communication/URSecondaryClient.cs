using System.Net;
using System.Net.Sockets;
using AutoInsert.Shared.Models;
using AutoInsert.Core.Utilities;

namespace AutoInsert.Core.Services.Communication;

// The secondary interface on the UR robot is for receiving data packages about the state of the robot.
public class URSecondaryClient(string ipAddress, int port = 30002)
{
    private Socket? _socket;
    private readonly string _ipAddress = ipAddress;
    private readonly int _port = port;
    private readonly URPackageParser _parser = new();
    public bool IsConnected => _socket?.Connected ?? false;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(_ipAddress);
            IPEndPoint endPoint = new IPEndPoint(ip, _port);

            await _socket.ConnectAsync(endPoint);
            Console.WriteLine("Connected to UR Secondary Server");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection to UR Secondary Server failed: {ex.Message}");
            return false;
        }
    }

    public async Task<Waypoint?> GetJointPositionsAsync()
    {
        // Flush old buffered data first to get latest position
        await FlushSocketBufferAsync();
        
        // Keep reading packages until we find one with Joint Data
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var packageData = await ReceivePackageAsync();
            
            if (packageData == null)
                continue;

            var waypoint = _parser.ParseJointPositions(packageData);
            if (waypoint != null)
                return waypoint;
            
            await Task.Delay(50);
        }
        
        return null;
    }

    public async Task<ToolData?> GetToolDataAsync()
    {
        // Flush old buffered data first
        await FlushSocketBufferAsync();
        
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var packageData = await ReceivePackageAsync();
            
            if (packageData == null)
                continue;

            var toolData = _parser.ParseToolData(packageData);
            if (toolData != null)
                return toolData;
            
            await Task.Delay(50);
        }
        
        return null;
    }

    private async Task FlushSocketBufferAsync()
    {
        if (!IsConnected || _socket == null)
            return;

        try
        {
            // Read and discard all available buffered data
            int bytesAvailable = _socket.Available;
            
            if (bytesAvailable > 0)
            {
                byte[] flushBuffer = new byte[bytesAvailable];
                await _socket.ReceiveAsync(new ArraySegment<byte>(flushBuffer), SocketFlags.None);
                Console.WriteLine($"[Secondary] Flushed {bytesAvailable} bytes of old data");
            }
            
            // Wait a bit for fresh data to arrive
            await Task.Delay(150); // Wait for at least one new package (10Hz = 100ms)
        }
        catch
        {
            // Ignore flush errors
        }
    }

    private async Task<byte[]?> ReceivePackageAsync()
    {
        if (!IsConnected || _socket == null)
            return null;

        try
        {
            byte[] sizeBuffer = new byte[4];
            int bytesRead = await ReceiveExactAsync(sizeBuffer, 0, 4);
            
            if (bytesRead < 4)
                return null;

            int packageSize = _parser.ReadInt32BigEndian(sizeBuffer, 0);
            
            if (packageSize < 10 || packageSize > 5000)
                return null;

            byte[] packageData = new byte[packageSize];
            Array.Copy(sizeBuffer, packageData, 4);
            
            bytesRead = await ReceiveExactAsync(packageData, 4, packageSize - 4);
            
            if (bytesRead < packageSize - 4)
                return null;
            
            return packageData;
        }
        catch
        {
            return null;
        }
    }

    private async Task<int> ReceiveExactAsync(byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        
        while (totalRead < count)
        {
            int bytesRead = await _socket!.ReceiveAsync(
                new ArraySegment<byte>(buffer, offset + totalRead, count - totalRead), 
                SocketFlags.None);
            
            if (bytesRead == 0)
                break;
                
            totalRead += bytesRead;
        }
        
        return totalRead;
    }

    public void Disconnect()
    {
        if (_socket != null)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            
            _socket.Close();
            _socket.Dispose();
            _socket = null;
        }
    }
}