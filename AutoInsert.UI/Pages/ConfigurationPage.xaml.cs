using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoInsert.UI.Pages;

public partial class ConfigurationPage : Page
{
    public ConfigurationPage()
    {
        InitializeComponent();
    }

    private ProgramConfiguration config = new("", 4);
    
    private void StartProgram(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new LoadingPage(config));
        
    }

    private void UploadProgram(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
    
    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
        e.Handled = !IsTextNumericAndPositive(newText);
    }

    private static bool IsTextNumericAndPositive(string text)
    {
        return int.TryParse(text, out int value) && value > 0;
    }
}