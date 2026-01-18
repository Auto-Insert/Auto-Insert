using System.IO.Ports;
using System.Runtime.InteropServices;
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
        private List<string> commandBuffer = new List<string>();
        
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
                    _serialPort.ReadBufferSize = 16384;
                    // _serialPort.DataReceived += SerialPort_DataReceived;
                    // _serialPort.ErrorReceived += SerialPort_ErrorReceived;
                    
                    _serialPort.Open();
                    _serialPort.DiscardInBuffer(); 
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
                    //_serialPort.DataReceived -= SerialPort_DataReceived;
                    //_serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                    _serialPort.Close();
                }
                _serialPort?.Dispose();
                _serialPort = null;
                CurrentPort = null;
            }
        }
        public bool AddCommandToBuffer(string command)
        {
            try
            {
                // if(commandBuffer.Count == 0)
                //     commandBuffer.Add("XXXXXXXXXXXXXXX");
                commandBuffer.Add(command);
                Console.WriteLine($"Buffered Command: {command}");
                Console.WriteLine($"Buffer Length: {commandBuffer.Count}");
                Console.WriteLine($"Full Buffer: {string.Join(", ", commandBuffer)}");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }
        public async Task<bool> SendCommandBufferAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        if (_serialPort?.IsOpen != true)
                            return false;
                        for (int i = 0; i < commandBuffer.Count; i++)
                        {
                            _serialPort.WriteLine(commandBuffer[i]);
                        }
                        Console.WriteLine($"Sent Command Buffer: {string.Join("\n", commandBuffer)}");
                        commandBuffer.Clear();
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
        public async Task<bool> AddDelayToBufferAsync(int delay)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        if (_serialPort?.IsOpen != true)
                            return false;
                        string delayCommand = $"XXXXXXXXXXXXXXX";
                        for (int i = 0; i < delay; i++)
                        {
                            commandBuffer.Add(delayCommand);
                        }
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
    
        // private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        // {
        //     try
        //     {
        //         if (_serialPort?.IsOpen == true && _serialPort.BytesToRead > 0)
        //         {
        //             string data = _serialPort.ReadExisting();
        //             DataReceived?.Invoke(this, data);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         ErrorOccurred?.Invoke(this, ex);
        //     }
        // }
        
        // private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        // {
        //     ErrorOccurred?.Invoke(this, new Exception($"Serial port error: {e.EventType}"));
        // }
        
        public void Dispose()
        {
            Disconnect();
        }
        public async Task<string?> WaitForStringAsync(string target, CancellationToken? cancellationToken = null)
        {
            if (_serialPort?.IsOpen != true)
                return null;

            while (_serialPort.IsOpen && (cancellationToken?.IsCancellationRequested != true))
            {
                try
                {
                    string data = _serialPort.ReadLine();
                    Console.WriteLine($"{data}");
                    if (data.Contains(target))
                    {
                        return data;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                }
            }
            return null;
            
        }
        public async Task ReadAndPrintContinuouslyAsync(CancellationToken? cancellationToken = null)
        {
            if (_serialPort?.IsOpen != true)
                return;

            while (_serialPort.IsOpen && (cancellationToken?.IsCancellationRequested != true))
            {
                try
                {
                    string data = _serialPort.ReadLine();
                    Console.WriteLine(data);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                }
            }
        }
    }
}