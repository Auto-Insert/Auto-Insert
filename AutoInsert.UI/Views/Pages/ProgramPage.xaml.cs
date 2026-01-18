using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;
using System.Diagnostics;
using AutoInsert.UI.ViewModels;
using AutoInsert.UI.Services;

namespace AutoInsert.UI.Pages;

public partial class ProgramPage : Page
{
    private readonly ProgramViewModel _viewModel;
    public ProgramPage()
    {
        InitializeComponent();
        _viewModel = new ProgramViewModel();
        DataContext = _viewModel;

    }
    private readonly SequenceController _sequenceController = new();

    private async void StartProgram(object sender, RoutedEventArgs e)
    {
        var loadedFileName = _viewModel.LoadedFileName ?? string.Empty;
        NavigationService.Navigate(new SequencePage(loadedFileName, _viewModel.Program));
    }

    private async void UploadProgram(object sender, RoutedEventArgs e)
    {
        var filePicker = new OpenFileDialog
        {
            Title = "Select program file",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };
        if (filePicker.ShowDialog() != true) return;
        _viewModel.ClearProgram();

        var pathToLoad = filePicker.FileName;
        var fileName = System.IO.Path.GetFileName(pathToLoad);

        try
        {
            var threadHoles = await ConfigurationService.LoadProgramAsync(pathToLoad);
            _viewModel.LoadProgram(fileName, threadHoles);
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = "An error occurred while loading the program file: " + ex.Message;
        }
    }
}