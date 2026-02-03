using System;
using System.Globalization;
using System.Windows.Data;
using VDLaser.ViewModels;

namespace VDLaser.Converters
{
    public class MachineStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MachineUiState state)
                return "Not connected"; ;

            return state switch
            {
                MachineUiState.Connecting => "Connecting...",
                MachineUiState.Connected => "Connected",
                MachineUiState.HomingRequired => "Homing required",
                MachineUiState.Homing => "Homing...",
                MachineUiState.Ready => "Ready",
                MachineUiState.Alarm => "Alarm",
                MachineUiState.EmergencyStop => "Emergency stop",
                _ => string.Empty
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
