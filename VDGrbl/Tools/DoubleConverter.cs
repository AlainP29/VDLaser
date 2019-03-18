using System;
using System.Globalization;
using System.Windows.Data;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Convert an object to double. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(object), typeof(double))]
    public class DoubleConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                //throw new InvalidOperationException("The target must be a double");
            }
            if(parameter!=null)
            {
                return System.Convert.ToDouble(value);
            }
            return System.Convert.ToDouble(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
