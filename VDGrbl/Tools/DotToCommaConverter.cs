﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace VDGrbl.Tools
{
    [ValueConversion(typeof(object), typeof(string))]
        public class DotToCommaConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
            if (targetType != typeof(string))
            {
                //throw new InvalidOperationException("The target must be a boolean");
            }
            string valueDot = (string)value;
            if (valueDot != null && valueDot.Contains("."))
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
