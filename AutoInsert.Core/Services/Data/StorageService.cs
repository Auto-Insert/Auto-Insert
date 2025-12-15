using System.Text.Json;
using System.Text.Json.Serialization;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Services.Data;

public class StorageService
{
    private readonly string _appDataPath;
    private readonly string _configFilePath;
    private readonly string _sequencesPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public StorageService(string appName = "AutoInsert")
    {
        // Store in AppData/Local/AutoInsert
        _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName
        );

        _configFilePath = Path.Combine(_appDataPath, "config.json");
        _sequencesPath = Path.Combine(_appDataPath, "Sequences");

        Directory.CreateDirectory(_appDataPath);
        Directory.CreateDirectory(_sequencesPath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
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

    // Sequence storage methods
    public async Task<bool> SaveSequenceAsync(Sequence sequence)
    {
        try
        {   
            string fileName = SanitizeFileName(sequence.Name) + ".json";
            string filePath = Path.Combine(_sequencesPath, fileName);
            
            string jsonString = JsonSerializer.Serialize(sequence, _jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving sequence: {ex.Message}");
            return false;
        }
    }

    public async Task<Sequence?> LoadSequenceAsync(string sequenceName)
    {
        try
        {
            string fileName = SanitizeFileName(sequenceName) + ".json";
            string filePath = Path.Combine(_sequencesPath, fileName);
            
            if (!File.Exists(filePath))
                return null;

            string jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<Sequence>(jsonString, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sequence: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Sequence>> LoadAllSequencesAsync()
    {
        var sequences = new List<Sequence>();
        
        try
        {
            var files = Directory.GetFiles(_sequencesPath, "*.json");
            
            foreach (var file in files)
            {
                try
                {
                    string jsonString = await File.ReadAllTextAsync(file);
                    var sequence = JsonSerializer.Deserialize<Sequence>(jsonString, _jsonOptions);
                    if (sequence != null)
                    {
                        sequences.Add(sequence);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading sequence from {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sequences: {ex.Message}");
        }
        
        return sequences;
    }

    public bool DeleteSequence(string sequenceName)
    {
        try
        {
            string fileName = SanitizeFileName(sequenceName) + ".json";
            string filePath = Path.Combine(_sequencesPath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting sequence: {ex.Message}");
            return false;
        }
    }

    public List<string> GetSequenceNames()
    {
        try
        {
            var files = Directory.GetFiles(_sequencesPath, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting sequence names: {ex.Message}");
            return new List<string>();
        }
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}