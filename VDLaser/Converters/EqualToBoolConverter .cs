using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class EqualToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Si CurrentTab et le paramètre sont des chaînes, on les compare directement
            string valStr = value.ToString()?.Replace(',', '.') ?? string.Empty;
            string paramStr = parameter.ToString()?.Replace(',', '.') ?? string.Empty;

            return string.Equals(valStr, paramStr, StringComparison.OrdinalIgnoreCase); // Comparaison sans tenir compte de la casse
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                try
                {
                    return System.Convert.ChangeType(parameter, targetType, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return Binding.DoNothing;
                }
            }

            return Binding.DoNothing;
        }
    }

}
