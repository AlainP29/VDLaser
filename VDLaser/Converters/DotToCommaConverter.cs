using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class DotToCommaConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object parameter, CultureInfo? culture)
        {
            return value is string str ? str.Replace('.', ',') : null;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is string str ? str.Replace(',', '.') : null;
        }
    }
}