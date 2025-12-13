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
    private readonly SemaphoreSlim _socketLock = new(1, 1); // Ensure only one operation at a time
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
        // Wait for exclusive access to the socket to prevent concurrent reads
        await _socketLock.WaitAsync();
        
        try
        {
            await FlushSocketBufferAsync();
            
            double[]? jointPositions = null;
            CartesianPositions? cartesianPositions = null;
            
            // Attempt multiple times to get valid data for both
            for (int attempt = 0; attempt < 15; attempt++)
            {
                var packageData = await ReceivePackageAsync();
                
                if (packageData == null)
                {
                    await Task.Delay(100);
                    continue;
                }

                // Try to parse both from the same package
                if (jointPositions == null)
                    jointPositions = _parser.ParseJointPositions(packageData);
                
                if (cartesianPositions == null)
                    cartesianPositions = _parser.ParseCartesianPositions(packageData);
                
                // If we have both, return
                if (jointPositions != null && cartesianPositions != null)
                    return (jointPositions, cartesianPositions);
                
                await Task.Delay(100);
            }
            
            return (jointPositions, cartesianPositions);
        }
        finally
        {
            _socketLock.Release();
        }
    }

    public async Task<double[]?> GetJointPositionsAsync()
    {
        await _socketLock.WaitAsync();
        try
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
        finally
        {
            _socketLock.Release();
        }
    } 
    public async Task<CartesianPositions?> GetCartesianPositionsAsync()
    {
        await _socketLock.WaitAsync();
        try
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
        finally
        {
            _socketLock.Release();
        }
    }
    public async Task<ToolData?> GetToolDataAsync()
    {
        await _socketLock.WaitAsync();
        try
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
        finally
        {
            _socketLock.Release();
        }
    }

    private async Task FlushSocketBufferAsync()
    {
        if (!IsConnected || _socket == null)
            return;

        try
        {
            int packagesRead = 0;
            var startTime = DateTime.Now;
            var maxFlushTime = TimeSpan.FromMilliseconds(50);
            
            while (DateTime.Now - startTime < maxFlushTime && _socket.Available > 1400)
            {
                try
                {
                    byte[] sizeBuffer = new byte[4];
                    int bytesRead = 0;
                    
                    // Read size header with timeout
                    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
                    try
                    {
                        bytesRead = await _socket.ReceiveAsync(new ArraySegment<byte>(sizeBuffer, 0, 4), SocketFlags.None, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    
                    if (bytesRead < 4)
                        break;
                    
                    int packageSize = _parser.ReadInt32BigEndian(sizeBuffer, 0);
                    
                    if (packageSize < 10 || packageSize > 5000)
                        break;
                    
                    // Read and discard the rest of the package
                    byte[] discardBuffer = new byte[packageSize - 4];
                    int totalRead = 0;
                    
                    using var cts2 = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
                    try
                    {
                        while (totalRead < discardBuffer.Length)
                        {
                            int read = await _socket.ReceiveAsync(
                                new ArraySegment<byte>(discardBuffer, totalRead, discardBuffer.Length - totalRead),
                                SocketFlags.None,
                                cts2.Token);
                            
                            if (read == 0)
                                break;
                                
                            totalRead += read;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    
                    if (totalRead < discardBuffer.Length)
                        break;
                    
                    packagesRead++;
                }
                catch
                {
                    break;
                }
            }
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
        var timeout = TimeSpan.FromSeconds(2);
        
        using var cts = new CancellationTokenSource(timeout);
        
        try
        {
            while (totalRead < count)
            {
                int bytesRead = await _socket!.ReceiveAsync(
                    new ArraySegment<byte>(buffer, offset + totalRead, count - totalRead), 
                    SocketFlags.None,
                    cts.Token);
                
                if (bytesRead == 0)
                    break;
                    
                totalRead += bytesRead;
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout occurred
        }
        catch
        {
            // Other errors
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