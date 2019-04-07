using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Convert a bool to a brushcolor. Use in Xaml.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                //throw new InvalidOperationException("The target must be a boolean");
            }
            bool b = (bool)value;
            return b ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.OrangeRed);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
