using System;
using System.Globalization;
using System.Windows.Data;
using NLog;

namespace VDLaser.Tools
{
    /// <summary>
    /// Convert an object (double) to percentage used in StringFormat=\{0:P\}. Use in Xaml laserView.
    /// </summary>
    [ValueConversion(typeof(object), typeof(double))]
    public class PercentageConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                logger.Error("PercentageConverter|The target must be a double");
            }
            if (value != null)
            {
                double b = (double)value;

                if (parameter != null)//Parameter is a factor: max value/parameter=100
                {
                    int p = System.Convert.ToInt32(parameter, CultureInfo.CurrentCulture);
                    return b / (100 * p);
                }
                return b / 100;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
