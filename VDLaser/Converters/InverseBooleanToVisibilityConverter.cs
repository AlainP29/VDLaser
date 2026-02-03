using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool isTrue)
            {
                // Inverse le booléen : true → false, false → true
                bool inverted = !isTrue;

                // Convertit en Visibility : true → Visible, false → Collapsed
                return inverted ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;  // Fallback si pas bool (ex. : null)
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is Visibility visibility)
            {
                // Inverse la logique pour back : Visible → false (car inversé → true → Visible)
                bool inverted = visibility == Visibility.Visible;
                return !inverted;  // Retourne le booléen original inversé
            }
            return false;  // Fallback
        }
    }
}