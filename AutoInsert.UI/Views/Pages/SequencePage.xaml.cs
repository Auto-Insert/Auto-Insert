using System.Windows.Controls;
using System.Windows.Navigation;
using AutoInsert.UI.ViewModels;
using AutoInsert.Shared.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace AutoInsert.UI.Pages;

public partial class SequencePage : Page
{
    private readonly SequenceViewModel _viewModel;
    private readonly AutoInsert.Core.Controllers.SequenceController _sequenceController = new();
    public SequencePage(string loadedFileName, ObservableCollection<ThreadHole> program)
    {
        InitializeComponent();
        _viewModel = new SequenceViewModel
        {
            LoadedFileName = loadedFileName,
            Program = program
        };
        DataContext = _viewModel;
        Loaded += SequencePage_Loaded;
    }

    private async void SequencePage_Loaded(object sender, RoutedEventArgs e)
    {
        ConnectingOverlay.Visibility = Visibility.Visible;
        MainContentPanel.IsEnabled = false;
        await Task.Delay(50); // Allow spinner to render
        var sequence = await _sequenceController.LoadHardcodedSequence();
        _viewModel.SequenceName = sequence.Name;
        _viewModel.SequenceDescription = sequence.Description;
        _viewModel.SequenceSteps = new ObservableCollection<SequenceStep>(sequence.Steps);
        ConnectingOverlay.Visibility = Visibility.Collapsed;
        MainContentPanel.IsEnabled = true;
    }

    // Sequence selection logic removed

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService != null && NavigationService.CanGoBack)
            NavigationService.GoBack();
        else
            MessageBox.Show("No previous page to go back to.");
    }

    private void RunSequenceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_viewModel.SequenceName) && _viewModel.SequenceSteps != null)
        {
            var runPage = new RunSequencePage(_viewModel.SequenceName, _viewModel.SequenceDescription ?? string.Empty, _viewModel.SequenceSteps);
            NavigationService?.Navigate(runPage);
            // Start the sequence immediately after navigation
            // Use dispatcher to ensure navigation completes before starting
            Dispatcher.BeginInvoke(new Action(() => runPage.StartSequence()), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
