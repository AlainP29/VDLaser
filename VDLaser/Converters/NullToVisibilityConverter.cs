using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VDLaser.Converters // Adaptez le namespace à votre projet
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Si la valeur est nulle ou une chaîne vide, on cache l'élément
            if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}