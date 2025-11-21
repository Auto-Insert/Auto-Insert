using System.Net;
using System.Net.Sockets;

namespace AutoInsert.Core.Services.Communication;

// The primary interface on the UR robot is for sending URScript commands.
public class URPrimaryClient(string ipAddress, int port = 30001) : IURClient
{
    private Socket? _socket;
    private readonly string _ipAddress = ipAddress;
    private readonly int _port = port;
    public bool IsConnected => _socket?.Connected ?? false;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(_ipAddress);
            IPEndPoint endPoint = new IPEndPoint(ip, _port);

            await _socket.ConnectAsync(endPoint);
            Console.WriteLine("Connected to UR Primary Server");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection to UR Primary Server failed: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> SendURScriptAsync(string script)
    {
        if (!IsConnected || _socket == null)
        {
            Console.WriteLine("Not connected to UR Primary Server");
            return false;
        }

        try
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(script + "\n");
            await _socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send URScript: {ex.Message}");
            return false;
        }
    }
    public void Disconnect()
    {
        if (_socket != null)
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
        }
    }
}
