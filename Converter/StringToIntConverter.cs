using NLog;
using System;
using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converter
{
    /// <summary>
    /// String to int converter.
    /// </summary>
    public class StringToIntConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Convert a string to an int.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = 0;
            if (targetType != typeof(string))
            {
                logger.Error("StringToIntConverter|The target must be a string");
            }
            else
            {
                int index = ((string)value).IndexOf('.');
                intValue= int.Parse(((string)value).Remove(index));
            }
            return (int)intValue;
        }
        /// <summary>
        /// Convert an int to a string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = 0;

            if (targetType != typeof(double))
            {
                //throw new InvalidOperationException("The target is double");
            }
            else if (targetType != typeof(int))
            {
                //throw new InvalidOperationException("The target is a int");
            }
            else if (targetType != typeof(string))
            {
                //throw new InvalidOperationException("The target is a string");
            }
            return (int)intValue;
        }
    }
}
