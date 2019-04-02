
using System;
using System.Globalization;
using System.Windows.Data;


namespace VDGrbl.Tools
{
        /// <summary>
        /// Multiple boolean converter. Try to use in Xaml with two boolean...
        /// </summary>
        [ValueConversion(typeof(bool), typeof(bool))]
        public class MultiBooleanConverter : IMultiValueConverter
        {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool) && (bool)value == true)
                {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new InvalidOperationException();
            }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}
