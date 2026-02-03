using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VDLaser.Core.Grbl.Parsers;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is MachState status ? status switch
            {
                // État de repos : Prêt, pas de danger
                MachState.Idle => Brushes.LimeGreen,

                // Mouvement actif : Cycle en cours ou Jogging manuel
                MachState.Run => Brushes.DodgerBlue,
                MachState.Jog => Brushes.SkyBlue,

                // Danger / Blocage : Nécessite une intervention immédiate
                MachState.Alarm => Brushes.Red,
                MachState.Door => Brushes.DeepPink, // Sécurité physique (carter)

                // Suspension : La machine attend (Pause)
                MachState.Hold => Brushes.Orange,

                // Procédures spéciales
                MachState.Home => Brushes.DarkTurquoise,
                MachState.Check => Brushes.MediumPurple,

                // Inactivité ou déconnexion
                MachState.Sleep => Brushes.DimGray,
                MachState.Undefined => Brushes.Gray,

                _ => Brushes.DarkGray
            } : Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();  // One-way
        }
    }
}