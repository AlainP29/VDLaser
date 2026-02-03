using System;
using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string paramString)
                {
                    var parts = paramString.Split('|');
                    if (parts.Length == 2)
                    {
                        return boolValue ? parts[0] : parts[1];
                    }
                }
                return boolValue ? "True" : "False";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}