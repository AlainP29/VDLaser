using System;
using System.Globalization;
using System.Windows.Data;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Invert a boolean. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                //throw new InvalidOperationException("The target must be a boolean");
            }
            if (parameter != null)
            {
                return !System.Convert.ToBoolean(value);
            }
            return !System.Convert.ToBoolean(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
                throw new InvalidOperationException();
        }

    }
}
