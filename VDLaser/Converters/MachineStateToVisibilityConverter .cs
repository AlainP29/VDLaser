using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VDLaser.ViewModels;

namespace VDLaser.Converters
{
    public class MachineStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is MachineUiState
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
