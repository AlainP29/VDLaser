using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System;

namespace VDGrbl.Tools
{

    /// <summary>
    /// Convert a second to time.
    /// </summary>
    [ValueConversion(typeof(int), typeof(TimeSpan))]
    class IntToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(int))
            {
                //throw new InvalidOperationException("The target must be an integer");
            }
            int b = (int)value;
            return new TimeSpan(b);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
