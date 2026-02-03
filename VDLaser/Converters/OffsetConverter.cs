using System;
using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class OffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double pos && double.TryParse(parameter?.ToString(), out double offset))
            {
                return pos + offset;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}