using System;
using System.Globalization;
using System.Windows.Data;
using NLog;

namespace VDLaser.Tools
{
    [ValueConversion(typeof(object), typeof(string))]
        public class DotToCommaConverter : IValueConverter
        {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
