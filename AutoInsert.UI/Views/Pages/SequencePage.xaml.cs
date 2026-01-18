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
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void SequencePage_Loaded(object sender, RoutedEventArgs e)
    {
        ConnectingOverlay.Visibility = Visibility.Visible;
        MainContentPanel.IsEnabled = false;
        await Task.Delay(50); // Allow spinner to render
        await _sequenceController.InitializeAsync();
        var names = _sequenceController.GetSequenceNames();
        _viewModel.SequenceNames = new ObservableCollection<string>(names);
        ConnectingOverlay.Visibility = Visibility.Collapsed;
        MainContentPanel.IsEnabled = true;
    }

    private async void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.SelectedSequenceName))
        {
            if (!string.IsNullOrEmpty(_viewModel.SelectedSequenceName))
            {
                var sequence = await _sequenceController.LoadSequenceFromStorageAsync(_viewModel.SelectedSequenceName);
                _viewModel.SelectedSequenceSteps = sequence != null ? new ObservableCollection<SequenceStep>(sequence.Steps) : null;
            }
            else
            {
                _viewModel.SelectedSequenceSteps = null;
            }
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService != null && NavigationService.CanGoBack)
            NavigationService.GoBack();
        else
            MessageBox.Show("No previous page to go back to.");
    }

    private void RunSequenceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_viewModel.SelectedSequenceName) && _viewModel.SelectedSequenceSteps != null)
        {
            MessageBox.Show($"Running sequence: {_viewModel.SelectedSequenceName}");
        }
    }
}
