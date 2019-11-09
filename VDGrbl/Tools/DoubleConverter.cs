using System;
using System.Globalization;
using System.Windows.Data;
using NLog;

namespace VDLaser.Tools
{
    /// <summary>
    /// Convert an object to double. Use in Xaml GCodeFileView.
    /// </summary>
    [ValueConversion(typeof(object), typeof(double))]
    public class DoubleConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                logger.Error("DoubleConverter|The target must be a double");
            }
            if (value != null)
            {
                if (parameter != null)
                {
                    return System.Convert.ToDouble(value, CultureInfo.CurrentCulture);
                }
                return System.Convert.ToDouble(value, CultureInfo.CurrentCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
