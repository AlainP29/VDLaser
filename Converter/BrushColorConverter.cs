using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NLog;

namespace VDLaser.Converter
{
    /// <summary>
    /// Bool to brushcolor converter. Use in Xaml SerialPortSettingView
    /// </summary>
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BrushColorConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Convert a bool to a brushcolor.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                logger.Error("BrushColorConverter|The target must be a boolean");
            }
            if (value != null)
            {
                bool b = (bool)value;
                if (parameter != null)
                {
                    return b ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.OrangeRed);
                }
                return b ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.OrangeRed);
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
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
