using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VDLaser.ViewModels;

namespace VDLaser.Converters
{
    public class IsHomingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MachineUiState state)
            {
                return state == MachineUiState.Homing
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
