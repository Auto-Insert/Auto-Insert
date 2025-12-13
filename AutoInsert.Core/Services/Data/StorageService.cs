using System.Text.Json;
using System.Text.Json.Serialization;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Data;

public class StorageService
{
    private readonly string _appDataPath;
    private readonly string _configFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public StorageService(string appName = "AutoInsert")
    {
        // Store in AppData/Local/AutoInsert
        _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName
        );

        _configFilePath = Path.Combine(_appDataPath, "config.json");

        Directory.CreateDirectory(_appDataPath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<bool> SaveConfigAsync<AppConfiguration>(AppConfiguration config)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
            return false;
        }
    }
    public async Task<AppConfiguration> LoadConfigAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
                return new AppConfiguration();

            string jsonString = await File.ReadAllTextAsync(_configFilePath);
            return JsonSerializer.Deserialize<AppConfiguration>(jsonString, _jsonOptions) ?? new AppConfiguration();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
            return new AppConfiguration();
        }
    }
    public bool ConfigExists() => File.Exists(_configFilePath);
    public string GetStoragePath() => _appDataPath;
}
