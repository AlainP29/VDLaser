using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using VDLaser.Core.Gcode;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Représente une ligne de G-Code analysée pour l'affichage dans l'interface utilisateur.
    /// </summary>
    public sealed partial class GcodeItemViewModel : ViewModelBase
    {
        #region Fields & Properties
        private readonly ILogService _log;
        public int LineNumber { get; }
        public string RawLine { get; }

        public GcodeCommand Command { get; }
        /// <summary>
        /// Indique si cette ligne a été envoyée avec succès au contrôleur GRBL.
        /// Utilisation d'ObservableProperty pour mettre à jour l'UI (ex: icône de validation).
        /// </summary>
        [ObservableProperty]
        private bool _isSent;
        public Point? StartPoint { get; set; }
        public Point? EndPoint { get; set; }
        public bool IsRapid { get; set; }

        #endregion

        public GcodeItemViewModel(int lineNumber, string rawLine, GcodeCommand command, ILogService log)
        {
            LineNumber = lineNumber;
            RawLine = rawLine ?? string.Empty;
            Command = command ?? throw new ArgumentNullException(nameof(command));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrEmpty(G) && string.IsNullOrEmpty(M))
            {
                _log.Warning("[GcodeItem] Ligne {Line} sans commande G ou M détectée : {Raw}", LineNumber, RawLine);
            }
        }

        #region UI Display Properties
        // Ces propriétés formatent les données pour le DataGrid (Culture Invariant pour éviter les conflits virgule/point)
        public string X => Command.X?.ToString("0.###", CultureInfo.InvariantCulture) ?? "";
        public string Y => Command.Y?.ToString("0.###", CultureInfo.InvariantCulture) ?? "";
        public string F => Command.F?.ToString("0", CultureInfo.InvariantCulture) ?? "";
        public string S => Command.S?.ToString("0", CultureInfo.InvariantCulture) ?? "";
        public string G => Command.G?.ToString() ?? "";
        public string M => Command.M?.ToString() ?? "";
        #endregion

        /// <summary>
        /// Notifie le système que la ligne a été traitée.
        /// </summary>
        public void MarkAsSent()
        {
            if (!IsSent)
            {
                IsSent = true;
                _log.Debug("[GcodeItem] Ligne {Line} envoyée au contrôleur.", LineNumber);
            }
        }
    }

}
