using System.Windows;
using System.Windows.Controls;

namespace AutoInsert.UI.Pages;

public partial class LoadingPage : Page
{
    private readonly ProgramConfiguration _config;
    private ProcessResult? _result;

    public LoadingPage(ProgramConfiguration config)
    {
        InitializeComponent();
        _config = config;
        Loaded += LoadingPage_Loaded;
    }

    private async void LoadingPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Start the process
        _result = await RunProcessAsync();
        
        // Navigate to completion page
        NavigationService?.Navigate(new CompletionPage(_result));
    }

    private async Task<ProcessResult> RunProcessAsync()
    {
        // Your process logic here
        await Task.Run(async () =>
        {
            // Update status on UI thread
            Dispatcher.Invoke((Action)(() => StatusText.Text = "Step 1: Loading program..."));
            await Task.Delay(1000);
            
            Dispatcher.Invoke((Action)(() => StatusText.Text = "Step 2: Processing blocks..."));
            await Task.Delay(2000);
            
            Dispatcher.Invoke((Action)(() => StatusText.Text = "Step 3: Finalizing..."));
            await Task.Delay(1000);
        });

        return new ProcessResult { Success = true, BlocksProcessed = _config.BlockCount };
    }
}