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
            int b = (int)value;

            if (targetType != typeof(int))
            {
                //throw new InvalidOperationException("The target must be an integer");
            }
            if (parameter != null)
            {
                return new TimeSpan(b);
            }
            return new TimeSpan(b);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
