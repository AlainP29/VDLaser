using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VDLaser.Core.Console;

namespace VDLaser.Converters
{
    public class ConsoleTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ConsoleMessageType type)
                return Brushes.White;

            return type switch
            {
                ConsoleMessageType.Info => Brushes.LightGray,
                ConsoleMessageType.Success => Brushes.LimeGreen,
                ConsoleMessageType.Warning => Brushes.Gold,
                ConsoleMessageType.Error => Brushes.OrangeRed,
                ConsoleMessageType.Alarm => Brushes.Red,
                ConsoleMessageType.System => Brushes.Cyan,
                _ => Brushes.White
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
