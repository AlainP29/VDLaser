using System;
using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class NegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) return -d;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) return -d;
            return value;
        }
    }
}