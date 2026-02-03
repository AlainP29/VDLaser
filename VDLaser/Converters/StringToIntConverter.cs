using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class StringToIntConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str && int.TryParse(str.Split('.')[0], out int result))
            {
                return result;
            }
            return 0;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value?.ToString() ?? "0";
        }
    }
}