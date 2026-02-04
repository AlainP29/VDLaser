using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VDLaser.Core.Grbl.Models;

namespace VDLaser.Converters
{
    public class SeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlarmSeverity severity)
            {
                return severity switch
                {
                    AlarmSeverity.Critical => Brushes.Red,
                    AlarmSeverity.Warning => Brushes.Orange,
                    _ => Brushes.Gray
                };
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
