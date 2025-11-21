using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AutoInsert.Core.Services.Communication;

// The dashboard interface on the UR robot is for simple commands like start, stop, and also for getting simple status.
public class URDashboardClient(string ipAddress, int port = 29999) : IURClient
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
            IPEndPoint endPoint = new(ip, _port);

            await _socket.ConnectAsync(endPoint);
            
            // Read connection message
            byte[] buffer = new byte[1024];
            int bytesRead = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            string welcome = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
            Console.WriteLine($"{welcome}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return false;
        }
    }
    public async Task<string> SendCommandAsync(string command)
    {
        if (!IsConnected || _socket == null)
        {
            Console.WriteLine("Not connected to robot");
            return string.Empty;
        }

        try
        {
            // Send command with newline terminator
            byte[] data = Encoding.ASCII.GetBytes(command + "\n");
            await _socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

            // Receive response
            byte[] buffer = new byte[1024];
            int bytesRead = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

            if (bytesRead > 0)
            {
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                return response;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send command error: {ex.Message}");
            return string.Empty;
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
