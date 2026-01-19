using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.ViewModels;

public class StepViewModel : INotifyPropertyChanged
{
    public SequenceStep Step { get; }
    public string Name => Step.Name;
    public string? Description => Step.Description;

    private bool _isCompleted;
    public bool IsCompleted
    {
        get => _isCompleted;
        set { _isCompleted = value; OnPropertyChanged(); }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    private bool _isFailed;
    public bool IsFailed
    {
        get => _isFailed;
        set { _isFailed = value; OnPropertyChanged(); }
    }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    public StepViewModel(SequenceStep step)
    {
        Step = step;
        IsEnabled = false;
        IsCompleted = false;
        IsRunning = false;
        IsFailed = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
