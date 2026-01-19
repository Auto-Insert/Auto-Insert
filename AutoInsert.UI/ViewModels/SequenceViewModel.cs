using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.ViewModels;

public class SequenceViewModel : INotifyPropertyChanged
{
    private string? _sequenceName;
    public string? SequenceName
    {
        get => _sequenceName;
        set { _sequenceName = value; OnPropertyChanged(); }
    }

    private string? _sequenceDescription;
    public string? SequenceDescription
    {
        get => _sequenceDescription;
        set { _sequenceDescription = value; OnPropertyChanged(); }
    }

    private ObservableCollection<SequenceStep>? _sequenceSteps;
    public ObservableCollection<SequenceStep>? SequenceSteps
    {
        get => _sequenceSteps;
        set { _sequenceSteps = value; OnPropertyChanged(); }
    }
    private string? _loadedFileName;
    public string? LoadedFileName
    {
        get => _loadedFileName;
        set { _loadedFileName = value; OnPropertyChanged(); }
    }
    private ObservableCollection<ThreadHole>? _program;
    public ObservableCollection<ThreadHole>? Program
    {
        get => _program;
        set { _program = value; OnPropertyChanged(); }
    }
    private ObservableCollection<string>? _sequenceNames;
    public ObservableCollection<string>? SequenceNames
    {
        get => _sequenceNames;
        set { _sequenceNames = value; OnPropertyChanged(); }
    }

    private string? _selectedSequenceName;
    public string? SelectedSequenceName
    {
        get => _selectedSequenceName;
        set { _selectedSequenceName = value; OnPropertyChanged(); }
    }

    private ObservableCollection<SequenceStep>? _selectedSequenceSteps;
    public ObservableCollection<SequenceStep>? SelectedSequenceSteps
    {
        get => _selectedSequenceSteps;
        set { _selectedSequenceSteps = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
