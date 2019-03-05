using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VDGrbl.Tools
{
    class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = 0;
            if (targetType != typeof(string))
            {
                //throw new InvalidOperationException("The target must be a string");
            }
            else
            {
                int index = ((string)value).IndexOf('.');
                intValue= int.Parse(((string)value).Remove(index));
            }
            return (int)intValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = 0;

            if (targetType != typeof(double))
            {
                //throw new InvalidOperationException("The target is double");
            }
            else if (targetType != typeof(int))
            {
                //throw new InvalidOperationException("The target is a int");
            }
            else if (targetType != typeof(string))
            {
                //throw new InvalidOperationException("The target is a string");
            }
            return (int)intValue;
        }
    }
}
