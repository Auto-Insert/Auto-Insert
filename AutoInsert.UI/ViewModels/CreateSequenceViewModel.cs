using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.ViewModels;

public class CreateSequenceListPageViewModel : INotifyPropertyChanged
{
    private readonly SequenceController _sequenceController;
    private ObservableCollection<string> _stepTypes = new();
    public ObservableCollection<string> StepTypes
    {
        get => _stepTypes;
        set
        {
            if (_stepTypes != value)
            {
                _stepTypes = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _selectedStepType;
    public string? SelectedStepType
    {
        get => _selectedStepType;
        set
        {
            if (_selectedStepType != value)
            {
                _selectedStepType = value;
                OnPropertyChanged();
            }
        }
    }

    private ObservableCollection<SequenceStep> _steps = new();
    public ObservableCollection<SequenceStep> Steps
    {
        get => _steps;
        set
        {
            if (_steps != value)
            {
                _steps = value;
                OnPropertyChanged();
            }
        }
    }

    public CreateSequenceListPageViewModel()
    {
        _sequenceController = new SequenceController();
        StepTypes = new ObservableCollection<string>();
        Steps = new ObservableCollection<SequenceStep>();
    }

    public async Task GetStepTypes()
    {
        await _sequenceController.InitializeAsync();
        var all = _sequenceController.GetAvailableStepTypes();
        StepTypes.Clear();
        foreach (var stepType in all)
            StepTypes.Add(stepType.ToString());
    }

    public void AddStep()
    {
        if (string.IsNullOrWhiteSpace(SelectedStepType)) return;
        var type = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == SelectedStepType && typeof(SequenceStep).IsAssignableFrom(t));
        if (type != null)
        {
            var instance = (SequenceStep?)System.Activator.CreateInstance(type);
            if (instance != null)
                Steps.Add(instance);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
