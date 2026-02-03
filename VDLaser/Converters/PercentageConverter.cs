using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    [ValueConversion(typeof(object), typeof(double))]
    public class PercentageConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is double b)
            {
                if (parameter is string p && int.TryParse(p, out int factor))
                    return b / (100 * factor);
                return b / 100;
            }
            return 0.0;
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is double perc)
            {
                if (parameter is string p && int.TryParse(p, out int factor))
                    return perc * (100 * factor);
                return perc * 100;
            }
            return 0.0;
        }
    }
}