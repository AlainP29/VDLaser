using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System;
using NLog;

namespace VDLaser.Converter
{

    /// <summary>
    /// Second to time converter.
    /// </summary>
    [ValueConversion(typeof(int), typeof(TimeSpan))]
    public class IntToTimeConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Convert a second to time.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int b = (int)value*10000000;

            if (targetType != typeof(int))
            {
                logger.Error("IntToTimeConverter|The target must be an integer");

            }
            if (parameter != null)
            {
                return new TimeSpan(b);
            }
            return new TimeSpan(b);
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
