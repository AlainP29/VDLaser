using System;
using System.Globalization;
using System.Windows.Data;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Convert an object to a string. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class StringConverter : IValueConverter
    {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value?.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
    }
}
