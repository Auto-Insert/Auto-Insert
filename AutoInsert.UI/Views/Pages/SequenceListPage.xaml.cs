using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;
using AutoInsert.UI.ViewModels;

namespace AutoInsert.UI.Pages;

public partial class SequenceListPage : Page
{
    private readonly SequenceListPageViewModel _viewModel;

    public SequenceListPage()
    {
        InitializeComponent();
        _viewModel = new SequenceListPageViewModel();
        DataContext = _viewModel;
        Loaded += SequenceListPage_Loaded;
    }

    private async void SequenceListPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
       await _viewModel.LoadSequencesAsync();
    }

    private void CreateSequenceButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        NavigationService?.Navigate(new CreateSequencePage());
    }
}
