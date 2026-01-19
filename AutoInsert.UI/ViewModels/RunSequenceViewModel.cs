using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoInsert.UI.ViewModels;

public class RunSequenceViewModel : INotifyPropertyChanged
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

    private ObservableCollection<StepViewModel> _stepViewModels = new();
    public ObservableCollection<StepViewModel> StepViewModels
    {
        get => _stepViewModels;
        set { _stepViewModels = value; OnPropertyChanged(); }
    }

    private bool _canStartSequence = true;
    public bool CanStartSequence
    {
        get => _canStartSequence;
        set { _canStartSequence = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
