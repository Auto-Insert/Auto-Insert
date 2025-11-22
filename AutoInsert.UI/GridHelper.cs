using System.Windows;
using System.Windows.Controls;

namespace AutoInsert.UI
{
    /// <summary>
    /// <b>This Class was generated using Claude 4.5 Sonnet.</b> <br/>
    /// Provides attached properties and helper methods to enhance the functionality of Grid elements.
    /// </summary>
    public static class GridHelper
    {
        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.RegisterAttached(
                "ColumnSpacing",
                typeof(double),
                typeof(GridHelper),
                new PropertyMetadata(0.0, OnColumnSpacingChanged));

        public static double GetColumnSpacing(DependencyObject obj)
        {
            return (double)obj.GetValue(ColumnSpacingProperty);
        }

        public static void SetColumnSpacing(DependencyObject obj, double value)
        {
            obj.SetValue(ColumnSpacingProperty, value);
        }

        private static void OnColumnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Grid grid && e.NewValue is double spacing and > 0)
            {
                // Apply immediately if already loaded
                if (grid.IsLoaded)
                {
                    ApplyColumnSpacing(grid, spacing);
                }
                else
                {
                    // Otherwise wait for loaded event
                    grid.Loaded += (sender, args) => ApplyColumnSpacing(grid, spacing);
                }
            }
        }

        private static void ApplyColumnSpacing(Grid grid, double spacing)
        {
            foreach (UIElement child in grid.Children)
            {
                if (child is FrameworkElement element)
                {
                    int column = Grid.GetColumn(element);
                    if (column > 0)
                    {
                        var margin = element.Margin;
                        element.Margin = new Thickness(margin.Left + spacing, margin.Top, margin.Right, margin.Bottom);
                    }
                }
            }
        }
    }
}