using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoInsert.UI.Converters
{
    public class Float6Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d.ToString("F6", culture);
            if (value is float f)
                return f.ToString("F6", culture);
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0.0;
            var str = value.ToString();
            if (double.TryParse(str, NumberStyles.Float, culture, out double d))
                return d;
            return 0.0;
        }
    }
}
