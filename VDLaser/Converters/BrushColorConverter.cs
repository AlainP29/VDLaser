using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VDLaser.Converters
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BrushColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is bool b ? (b ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.OrangeRed)) : null;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}