using System.Windows.Controls;
using AutoInsert.UI.ViewModels;

namespace AutoInsert.UI.Pages;

public partial class CreateSequencePage : Page
{
    private CreateSequenceListPageViewModel viewModel;
    public CreateSequencePage()
    {
        viewModel = new CreateSequenceListPageViewModel();
        InitializeComponent();
        DataContext = viewModel;
        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (viewModel != null)
        {
            await viewModel.GetStepTypes();
        }
    }
    private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        NavigationService?.Navigate(new SequenceListPage());
    }
    private void AddStepButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        viewModel.AddStep();
    }
}
