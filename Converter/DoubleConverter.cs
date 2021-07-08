using System;
using System.Globalization;
using System.Windows.Data;
using NLog;

namespace VDLaser.Converter
{
    /// <summary>
    /// Object to double converter. Use in Xaml GCodeFileView.
    /// </summary>
    [ValueConversion(typeof(object), typeof(double))]
    public class DoubleConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Convert an object to double.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
