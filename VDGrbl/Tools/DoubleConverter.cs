using System;
using System.Globalization;
using System.Windows.Data;

namespace VDGrbl.Tools
{
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
                return System.Convert.ToDouble(value) / 100;
            }
            return System.Convert.ToDouble(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
