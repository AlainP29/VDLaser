using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class TabToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 'value' est le CurrentTab du ViewModel
            // 'parameter' est le nom de l'onglet défini dans le XAML
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            return value.ToString() == parameter.ToString()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}