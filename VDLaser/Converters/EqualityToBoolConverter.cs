using System;
using System.Globalization;
using System.Windows.Data;

namespace VDLaser.Converters
{
    public class EqualityToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Du ViewModel vers la Vue (Vérifie si le bouton doit être coché)
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string valStr = value.ToString()?.Replace(',', '.') ?? "0";
            string paramStr = parameter.ToString()?.Replace(',', '.') ?? "0";

            if (double.TryParse(value.ToString().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double v) &&
                    double.TryParse(parameter.ToString().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double p))
            {
                return Math.Abs(v - p) < 0.001; // Comparaison avec une petite tolérance
            }
            return false;
        }

        /// <summary>
        /// De la Vue vers le ViewModel (Met à jour la valeur quand on clique)
        /// </summary>
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