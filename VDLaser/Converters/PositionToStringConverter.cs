using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class PositionToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is double position)
            {
                string axis = parameter as string ?? "";  // Parameter pour spécifier "X:", "Y:", etc.
                return $"{axis}{position:F2} mm";  // Format à 2 décimales
            }
            return "N/A";
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str && double.TryParse(str.Replace(" mm", ""), out double pos))
            {
                return pos;
            }
            return 0.0;
        }
    }
}