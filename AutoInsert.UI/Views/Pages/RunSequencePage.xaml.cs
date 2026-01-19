using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading.Tasks;
using AutoInsert.UI.ViewModels;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.Pages;

public partial class RunSequencePage : Page
{
    private readonly RunSequenceViewModel _viewModel;
    private readonly AutoInsert.Core.Controllers.SequenceController _sequenceController = new();

    public RunSequencePage(string sequenceName, string sequenceDescription, ObservableCollection<SequenceStep> steps)
    {
        InitializeComponent();
        _viewModel = new RunSequenceViewModel
        {
            SequenceName = sequenceName,
            SequenceDescription = sequenceDescription,
            StepViewModels = new ObservableCollection<StepViewModel>()
        };
        foreach (var step in steps)
        {
            _viewModel.StepViewModels.Add(new StepViewModel(step));
        }
        DataContext = _viewModel;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService != null && NavigationService.CanGoBack)
            NavigationService.GoBack();
        else
            MessageBox.Show("No previous page to go back to.");
    }

    public async void StartSequence()
    {
        _viewModel.CanStartSequence = false;
        await Task.Run(async () =>
        {
            await _sequenceController.InitializeAsync();
            Application.Current.Dispatcher.Invoke(() => {
                _sequenceController.StepStarted += SequenceController_StepStarted;
                _sequenceController.StepCompleted += SequenceController_StepCompleted;
                _sequenceController.StepFailed += SequenceController_StepFailed;
            });
            var sequence = new Sequence
            {
                Name = _viewModel.SequenceName,
                Description = _viewModel.SequenceDescription,
                Steps = new List<SequenceStep>(_viewModel.StepViewModels.Select(vm => vm.Step))
            };
            await _sequenceController.ExecuteSequenceAsync(sequence);
            Application.Current.Dispatcher.Invoke(() => _viewModel.CanStartSequence = true);
        });
    }

    private async void StartSequenceButton_Click(object sender, RoutedEventArgs e)
    {
        StartSequence();
    }

    private void SequenceController_StepStarted(object? sender, SequenceStep e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            bool found = false;
            int runningIndex = -1;
            for (int i = 0; i < _viewModel.StepViewModels.Count; i++)
            {
                var vm = _viewModel.StepViewModels[i];
                if (!found && ReferenceEquals(vm.Step, e))
                {
                    vm.IsRunning = true;
                    vm.IsEnabled = true;
                    found = true;
                    runningIndex = i;
                }
                else
                {
                    vm.IsRunning = false;
                    vm.IsEnabled = false;
                }
            }
            // Auto-scroll to running step
            if (runningIndex >= 0 && StepsItemsControl.ItemContainerGenerator.ContainerFromIndex(runningIndex) is FrameworkElement fe)
            {
                fe.BringIntoView();
            }
        });
    }

    private void SequenceController_StepCompleted(object? sender, SequenceStep e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < _viewModel.StepViewModels.Count; i++)
            {
                var vm = _viewModel.StepViewModels[i];
                if (ReferenceEquals(vm.Step, e))
                {
                    vm.IsCompleted = true;
                    vm.IsRunning = false;
                    vm.IsEnabled = false;
                    break;
                }
            }
        });
    }

    private void SequenceController_StepFailed(object? sender, SequenceStep e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < _viewModel.StepViewModels.Count; i++)
            {
                var vm = _viewModel.StepViewModels[i];
                if (ReferenceEquals(vm.Step, e))
                {
                    vm.IsFailed = true;
                    vm.IsRunning = false;
                    vm.IsEnabled = false;
                    break;
                }
            }
        });
    }
}
