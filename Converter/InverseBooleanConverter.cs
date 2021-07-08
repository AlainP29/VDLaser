﻿using System;
using System.Globalization;
using System.Windows.Data;
using NLog;

namespace VDLaser.Converter
{
    /// <summary>
    /// Invert a boolean. Use in Xaml DataConsoleView, GrblCommandViewer, JoggingView, sendDataView and laserView.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Invert a boolean.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                logger.Error("InverseBooleanConverter|The target must be a boolean");
            }
            if (value != null)
            {
            if (parameter != null)
                {
                    return !System.Convert.ToBoolean(value, CultureInfo.CurrentCulture);
                }
                else
                    return !System.Convert.ToBoolean(value, CultureInfo.CurrentCulture);
            }
            return null;
        }
        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
