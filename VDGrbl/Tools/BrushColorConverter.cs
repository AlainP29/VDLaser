using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NLog;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Convert a bool to a brushcolor. Use in Xaml SerialPortSettingView
    /// </summary>
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BrushColorConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            if (targetType != typeof(bool))
            {
                logger.Error("BrushColorConverter|The target must be a boolean");
            }
            if (parameter != null)
            {
                return b ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.OrangeRed);
            }
            return b ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.OrangeRed);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
