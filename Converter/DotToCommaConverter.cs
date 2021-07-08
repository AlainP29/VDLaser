using System;
using System.Globalization;
using System.Windows.Data;
using NLog;

namespace VDLaser.Converter
{
    /// <summary>
    /// Dot to comma converter.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
        public class DotToCommaConverter : IValueConverter
        {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Convert a dot to a comma.
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
                logger.Error("DotToCommaConverter|The target must be a string");
            }
            if (value != null)
            {
                string valueDot = (string)value;
                if (valueDot.Contains("."))
                {
                    string valueComma = valueDot.Replace('.', ',');
                    if (parameter != null)
                    {
                        return valueComma;
                    }
                    return valueComma;
                }
                return valueDot;
            }
            return null;
        }
        /// <summary>
        /// Not implemented yet
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
