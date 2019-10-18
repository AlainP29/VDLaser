using System;
using System.Globalization;
using System.Windows.Data;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Convert an object (double) to percentage used in StringFormat=\{0:P\}. Use in Xaml laserView.
    /// </summary>
    [ValueConversion(typeof(object), typeof(double))]
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double b = (double)value;

            if (targetType != typeof(double))
            {
                //throw new InvalidOperationException("The target must be a double");
            }
            if(parameter!=null)//Parameter is a factor: max value/parameter=100
            {
                int p = System.Convert.ToInt32(parameter,CultureInfo.CurrentCulture);
                return b/(100*p);
            }
            return b / 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
