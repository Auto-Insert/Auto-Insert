using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoInsert.UI.Converters;

public class RadiansToDegreeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double radians)
        {
            return radians * (180.0 / Math.PI);
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}