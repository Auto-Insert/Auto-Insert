using System.IO.Ports;
using System.Text;

namespace AutoInsert.Core.Services.Communication
{
    public class UartService : IDisposable
    {
        private SerialPort? _serialPort;
        private readonly object _lock = new();
        
        public bool IsConnected => _serialPort?.IsOpen ?? false;
        public string? CurrentPort { get; private set; }
        
        public event EventHandler<string>? DataReceived;
        public event EventHandler<Exception>? ErrorOccurred;
        
        public static string[] GetAvailablePorts() => SerialPort.GetPortNames();
        public bool Connect(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            try
            {
                lock (_lock)
                {
                    Disconnect();
                    
                    _serialPort = new SerialPort(portName, baudRate)
                    {
                        DataBits = dataBits,
                        Parity = parity,
                        StopBits = stopBits,
                        Handshake = Handshake.None,
                        ReadTimeout = 500,
                        WriteTimeout = 500,
                        Encoding = Encoding.ASCII
                    };
                    
                    _serialPort.DataReceived += SerialPort_DataReceived;
                    _serialPort.ErrorReceived += SerialPort_ErrorReceived;
                    
                    _serialPort.Open();
                    CurrentPort = portName;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }
        
        public void Disconnect()
        {
            lock (_lock)
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                    _serialPort.Close();
                }
                _serialPort?.Dispose();
                _serialPort = null;
                CurrentPort = null;
            }
        }
        
        public async Task<bool> SendCommandAsync(string command)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        if (_serialPort?.IsOpen != true)
                            return false;
                        
                        _serialPort.WriteLine(command);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                    return false;
                }
            });
        }    
        
        public async Task<string?> SendCommandWithResponseAsync(string command, int timeoutMs = 1000)
        {
            try
            {
                if (_serialPort?.IsOpen != true)
                    return null;
                
                _serialPort.DiscardInBuffer();
                _serialPort.WriteLine(command);
                
                var cts = new CancellationTokenSource(timeoutMs);
                return await Task.Run(() =>
                {
                    try
                    {
                        return _serialPort.ReadLine();
                    }
                    catch
                    {
                        return null;
                    }
                }, cts.Token);
            }
            catch
            {
                return null;
            }
        }
        
        public async Task<string?> ReadAvailableAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        if (_serialPort?.IsOpen == true && _serialPort.BytesToRead > 0)
                        {
                            return _serialPort.ReadExisting();
                        }
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                    return null;
                }
            });
        }
        
        public void ClearBuffers()
        {
            lock (_lock)
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                }
            }
        }
    
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort?.IsOpen == true && _serialPort.BytesToRead > 0)
                {
                    string data = _serialPort.ReadExisting();
                    DataReceived?.Invoke(this, data);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }
        
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ErrorOccurred?.Invoke(this, new Exception($"Serial port error: {e.EventType}"));
        }
        
        public void Dispose()
        {
            Disconnect();
        }
    }
}