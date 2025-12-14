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
    
    // Background reading fields
    private CancellationTokenSource? _cts;
    private Task? _readTask;
    private double[]? _latestJointPositions;
    private CartesianPositions? _latestCartesianPositions;
    private ToolData? _latestToolData;
    private DateTime _lastPackageReceived = DateTime.MinValue;
    
    public bool IsConnected => _socket?.Connected ?? false;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            Disconnect(); // Ensure clean state

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(_ipAddress);
            IPEndPoint endPoint = new IPEndPoint(ip, _port);

            await _socket.ConnectAsync(endPoint);
            
            // Start background reading
            _cts = new CancellationTokenSource();
            _readTask = Task.Run(() => ReadLoopAsync(_cts.Token));
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task ReadLoopAsync(CancellationToken token)
    {
        byte[] headerBuffer = new byte[4];
        
        try
        {
            while (!token.IsCancellationRequested && IsConnected)
            {
                // Read header
                int bytesRead = await ReceiveExactAsync(headerBuffer, 0, 4, token);
                if (bytesRead < 4) 
                {
                    break;
                }

                int packageSize = _parser.ReadInt32BigEndian(headerBuffer, 0);
                if (packageSize < 4 || packageSize > 20000) // Sanity check
                {
                     break;
                }

                byte[] packageData = new byte[packageSize];
                Array.Copy(headerBuffer, packageData, 4);
                
                bytesRead = await ReceiveExactAsync(packageData, 4, packageSize - 4, token);
                if (bytesRead < packageSize - 4) 
                {
                    break;
                }

                _lastPackageReceived = DateTime.Now;

                // Parse data
                var joints = _parser.ParseJointPositions(packageData);
                if (joints != null) _latestJointPositions = joints;

                var cartesian = _parser.ParseCartesianPositions(packageData);
                if (cartesian != null) _latestCartesianPositions = cartesian;

                var tool = _parser.ParseToolData(packageData);
                if (tool != null) _latestToolData = tool;
            }
        }
        catch (Exception)
        {
        }
    }

    private async Task<int> ReceiveExactAsync(byte[] buffer, int offset, int count, CancellationToken token)
    {
        int totalRead = 0;
        
        try
        {
            while (totalRead < count)
            {
                if (_socket == null || !IsConnected) return totalRead;

                int bytesRead = await _socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer, offset + totalRead, count - totalRead), 
                    SocketFlags.None,
                    token);
                
                if (bytesRead == 0)
                    break;
                    
                totalRead += bytesRead;
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        
        return totalRead;
    }

    public async Task<(double[]? jointPositions, CartesianPositions? cartesianPositions)> GetRobotStateAsync()
    {
        return await Task.FromResult((_latestJointPositions, _latestCartesianPositions));
    }

    public async Task<double[]?> GetJointPositionsAsync()
    {
        return await Task.FromResult(_latestJointPositions);
    } 

    public async Task<CartesianPositions?> GetCartesianPositionsAsync()
    {
        return await Task.FromResult(_latestCartesianPositions);
    }

    public async Task<ToolData?> GetToolDataAsync()
    {
        return await Task.FromResult(_latestToolData);
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        try { _readTask?.Wait(500); } catch { }
        
        if (_socket != null)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) 
            { 
            }
            
            _socket.Close();
            _socket.Dispose();
            _socket = null;
        }
    }
}