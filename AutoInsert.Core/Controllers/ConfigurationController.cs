using System.IO.Ports;
using AutoInsert.Core.Services.Data;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Controllers;

public class ConfigurationController
{
    private readonly StorageService _storageService;
    private AppConfiguration _currentConfig;

    public ConfigurationController(StorageService storageService)
    {
        _storageService = storageService;
        _currentConfig = new AppConfiguration();
    }

    public async Task<AppConfiguration> LoadConfigurationAsync()
    {
        _currentConfig = await _storageService.LoadConfigAsync();
        return _currentConfig;
    }

    public async Task<bool> SaveConfigurationAsync()
    {
        return await _storageService.SaveConfigAsync(_currentConfig);
    }

    public AppConfiguration GetConfiguration()
    {
        return _currentConfig;
    }

    public async Task<bool> SetRobotIpAddressAsync(string ipAddress)
    {
        _currentConfig.URRobotIpAddress = ipAddress;
        return await SaveConfigurationAsync();
    }

    public string? GetRobotIpAddress()
    {
        return _currentConfig.URRobotIpAddress;
    }

    public async Task<bool> SetSerialPortAsync(string portName, int baudRate = 9600)
    {
        _currentConfig.SerialPort = portName;
        _currentConfig.SerialBaudRate = baudRate;
        return await SaveConfigurationAsync();
    }

    public string? GetSerialPort()
    {
        return _currentConfig.SerialPort;
    }

    public int GetSerialBaudRate()
    {
        return _currentConfig.SerialBaudRate ?? 9600;
    }

    public static string[] GetAvailableSerialPorts()
    {
        return SerialPort.GetPortNames();
    }

    public bool HasCalibration()
    {
        return _currentConfig.IsCalibrated && _currentConfig.CalibrationData != null;
    }

    public DateTime? GetLastCalibrationTime()
    {
        return _currentConfig.LastCalibrationTime;
    }

    public async Task<bool> ResetToDefaultsAsync()
    {
        _currentConfig = new AppConfiguration();
        return await SaveConfigurationAsync();
    }

    public bool ConfigurationExists()
    {
        return _storageService.ConfigExists();
    }
}

