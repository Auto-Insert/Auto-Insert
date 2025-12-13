using System.Net;
using System.Net.Sockets;
using AutoInsert.Shared.Models;
using AutoInsert.Core.Utilities;

namespace AutoInsert.Core.Services.Communication;

// The secondary interface on the UR robot is for receiving data packages about the state of the robot.
public class URSecondaryClient(string ipAddress, int port = 30002): IURClient
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

    public async Task<(double[]? jointPositions, CartesianPositions? cartesianPositions)> GetRobotStateAsync()
    {
        // Flush old buffered data first to get latest position
        await FlushSocketBufferAsync();
        
        double[]? jointPositions = null;
        CartesianPositions? cartesianPositions = null;
        
        int packagesReceived = 0;
        
        // Attempt multiple times to get valid data for both
        for (int attempt = 0; attempt < 15; attempt++)
        {
            var packageData = await ReceivePackageAsync();
            
            if (packageData == null)
            {
                await Task.Delay(100);
                continue;
            }

            packagesReceived++;

            // Try to parse both from the same package
            if (jointPositions == null)
            {
                jointPositions = _parser.ParseJointPositions(packageData);
            }
            
            if (cartesianPositions == null)
            {
                cartesianPositions = _parser.ParseCartesianPositions(packageData);
            }
            
            // If we have both, return
            if (jointPositions != null && cartesianPositions != null)
            {
                return (jointPositions, cartesianPositions);
            }
            
            await Task.Delay(100);
        }
        
        return (jointPositions, cartesianPositions);
    }

    public async Task<double[]?> GetJointPositionsAsync()
    {
        // Flush old buffered data first to get latest position
        await FlushSocketBufferAsync();
        
        // Attempt multiple times to get valid data
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var packageData = await ReceivePackageAsync();
            
            if (packageData == null)
                continue;

            var jointPositions = _parser.ParseJointPositions(packageData);
            if (jointPositions != null)
            {
                return jointPositions;
            }
            await Task.Delay(150);
        }
        return null;
    } 
    public async Task<CartesianPositions?> GetCartesianPositionsAsync()
    {
        // Flush old buffered data first to get latest position
        await FlushSocketBufferAsync();
        
        // Attempt multiple times to get valid data
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var packageData = await ReceivePackageAsync();
            
            if (packageData == null)
                continue;

            var cartesianPositions = _parser.ParseCartesianPositions(packageData);
            if (cartesianPositions != null)
                return cartesianPositions;
            
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
            
            await Task.Delay(150);
        }
        
        return null;
    }

    private async Task FlushSocketBufferAsync()
    {
        if (!IsConnected || _socket == null)
            return;

        try
        {
            // Read and discard complete packages to stay synchronized
            // Read for a short time to get fresh data
            var timeout = DateTime.Now.AddMilliseconds(200);
            
            while (DateTime.Now < timeout && _socket.Available > 0)
            {
                // Read complete packages, not raw bytes, to maintain sync
                try
                {
                    byte[] sizeBuffer = new byte[4];
                    int bytesRead = 0;
                    
                    // Try to read size header
                    for (int i = 0; i < 4 && DateTime.Now < timeout; i++)
                    {
                        if (_socket.Available > 0)
                        {
                            bytesRead += await _socket.ReceiveAsync(new ArraySegment<byte>(sizeBuffer, bytesRead, 4 - bytesRead), SocketFlags.None);
                        }
                        else
                        {
                            await Task.Delay(10);
                        }
                    }
                    
                    if (bytesRead < 4)
                        break;
                    
                    int packageSize = _parser.ReadInt32BigEndian(sizeBuffer, 0);
                    
                    if (packageSize < 10 || packageSize > 5000)
                        break; // Invalid size, stop flushing
                    
                    // Read and discard the rest of the package
                    byte[] discardBuffer = new byte[packageSize - 4];
                    int totalRead = 0;
                    
                    while (totalRead < discardBuffer.Length && DateTime.Now < timeout)
                    {
                        if (_socket.Available > 0)
                        {
                            int read = await _socket.ReceiveAsync(
                                new ArraySegment<byte>(discardBuffer, totalRead, discardBuffer.Length - totalRead), 
                                SocketFlags.None);
                            totalRead += read;
                        }
                        else
                        {
                            await Task.Delay(10);
                        }
                    }
                }
                catch
                {
                    break;
                }
            }
            
            // Wait a bit for fresh data to arrive
            await Task.Delay(100);
        }
        catch
        {
            // Ignore errors during flush
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