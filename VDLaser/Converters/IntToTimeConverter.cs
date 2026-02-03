using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    [ValueConversion(typeof(int), typeof(TimeSpan))]
    public class IntToTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is int seconds ? TimeSpan.FromSeconds(seconds) : TimeSpan.Zero;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is TimeSpan ts ? (int)ts.TotalSeconds : 0;
        }
    }
}