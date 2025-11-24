using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoInsert.UI.Pages;

namespace AutoInsert.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Subscribe to global keyboard events
        this.PreviewKeyDown += MainWindow_PreviewKeyDown;
        
        // Start with configuration page
        MainFrame.Navigate(new ConfigurationPage());
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Press F12 to open debug page
        if (e.Key == Key.F12)
        {
            MainFrame.Navigate(new DebugPage());
            e.Handled = true;
        }
    }

    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        // Clear navigation history to prevent back button issues
        if (e.Content is CompletionPage)
        {
            // Keep history so user can go back from completion
            return;
        }
        
        // Remove back entries for other pages (one-way flow)
        while (MainFrame.CanGoBack)
        {
            MainFrame.RemoveBackEntry();
        }
    }
}