using System;
using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters // Adapte le namespace à ton projet
{
    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 'value' est le booléen IsInteracting
            bool isInteracting = (bool)value;

            // Si on déplace la souris, on épaissit un peu le trait (ex: 1.5)
            // Sinon, on garde un trait fin (ex: 0.8)
            return isInteracting ? 1.5 : 0.8;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}