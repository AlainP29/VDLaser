using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is bool b ? !b : (object)false;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return Convert(value, targetType, parameter, culture);  // Symétrique
        }
    }
}