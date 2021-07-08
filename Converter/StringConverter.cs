using NLog;
using System;
using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converter
{
    /// <summary>
    /// Object to string converter. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class StringConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
/// <summary>
/// Convert an object to a string.
/// </summary>
/// <param name="value"></param>
/// <param name="targetType"></param>
/// <param name="parameter"></param>
/// <param name="culture"></param>
/// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (targetType != typeof(object))
                {
                logger.Error("StringConverter|The target must be an object");
            }
            if (parameter != null)
                {
                   return value?.ToString();
                }
                return value?.ToString();
            }
        /// <summary>
        /// not implemented yet.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targettype"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
    }
}
