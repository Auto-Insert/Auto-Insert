using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.ViewModels;

public class ProgramViewModel() : INotifyPropertyChanged
{
    public int[] AvailableBlockCounts { get; } = { 1, 2, 3, 4 };
    private string? _errorMessage = null;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }
    private int _blockCount = 1;
    public int SelectedBlockCount
    {
        get => _blockCount;
        set
        {
            _blockCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RequiredPlugs));
            OnPropertyChanged(nameof(EstimatedTime));
        }
    }
    private bool _programLoaded = false;
    public bool ProgramLoaded
    {
        get => _programLoaded;
        set
        {
            _programLoaded = value;
            OnPropertyChanged();
        }
    }
    private string? _loadedFileName = null;
    public string? LoadedFileName
    {
        get => _loadedFileName;
        set
        {
            _loadedFileName = value;
            OnPropertyChanged();
        }
    }
    private ObservableCollection<ThreadHole>? _program = null;
    public ObservableCollection<ThreadHole>? Program
    {
        get => _program;
        set
        {
            _program = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RequiredPlugs)); 
            OnPropertyChanged(nameof(HoleCount));
            OnPropertyChanged(nameof(EstimatedTime));
        }
    }
    // Computed properties from the loaded program
    public int HoleCount => Program?.Count ?? 0;
    public Dictionary<string, int> RequiredPlugs
    {
        get
        {
            if (Program == null || Program.Count == 0)
                return new Dictionary<string, int>();

            return Program
                .GroupBy(hole => hole.PlugType)
                .ToDictionary(group => group.Key, group => group.Count() * _blockCount);
        }
    }
    
    public string EstimatedTime
    {
        get
        {
            if (Program == null || Program.Count == 0)
                return "N/A";

            double timePerHoleSeconds = 30.0; // Example fixed time per hole
            double totalSeconds = Program.Count * timePerHoleSeconds * _blockCount;

            TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
            return string.Format("{0:D2}m:{1:D2}s", (int)timeSpan.TotalMinutes, timeSpan.Seconds);
        }
    }
    
    // Visual representation methods
    public void LoadProgram(string fileName, List<ThreadHole> threadHoles)
    {
        if (threadHoles == null || threadHoles.Count == 0)
        {
            ProgramLoaded = false;
            ErrorMessage = "Failed to load the file. Make sure that the format is correct.";
            return;
        }
        else
        {
            ErrorMessage = null;
        }

        LoadedFileName = fileName;
        Program = new ObservableCollection<ThreadHole>(threadHoles);
        ProgramLoaded = threadHoles != null && threadHoles.Count > 0;
    }
    public void ClearProgram()
    {
        LoadedFileName = null;
        Program = null;
        ProgramLoaded = false;
        ErrorMessage = null;
    }
    
    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}