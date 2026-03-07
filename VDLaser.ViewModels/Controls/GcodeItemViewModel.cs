using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using System.Windows;
using VDLaser.Core.Gcode;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Represents a single parsed G-Code line within a job queue.
    /// </summary>
    public sealed partial class GcodeItemViewModel : ViewModelBase
    {
        #region Fields & Properties
        private readonly ILogService _log;

        public int LineNumber { get; }
        public string RawLine { get; }

        public GcodeCommand Command { get; }
        public Point? StartPoint { get; set; }
        public Point? EndPoint { get; set; }
        public bool IsRapid { get; set; }

        [ObservableProperty]
        private bool _isSent;
        #endregion

        public GcodeItemViewModel(int lineNumber, string rawLine, GcodeCommand command, ILogService log)
        {
            LineNumber = lineNumber;
            RawLine = rawLine ?? string.Empty;
            Command = command ?? throw new ArgumentNullException(nameof(command));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrEmpty(G) && string.IsNullOrEmpty(M))
            {
                _log.Warning("[GCODEITEM] Line {Line}: No G or M command detected. Raw: {Raw}", LineNumber, RawLine);
            }
        }

        #region UI Display Properties
        public string X => Command.X?.ToString("0.###", CultureInfo.InvariantCulture) ?? "";
        public string Y => Command.Y?.ToString("0.###", CultureInfo.InvariantCulture) ?? "";
        public string F => Command.F?.ToString("0", CultureInfo.InvariantCulture) ?? "";
        public string S => Command.S?.ToString("0", CultureInfo.InvariantCulture) ?? "";
        public string G => Command.G?.ToString() ?? "";
        public string M => Command.M?.ToString() ?? "";
        #endregion

        /// <summary>
        /// Marks the line as processed/sent to the controller.
        /// </summary>
        public void MarkAsSent()
        {
            if (!IsSent)
            {
                IsSent = true;
                _log.Debug("[GCODEITEM] Line {Line} marked as sent.", LineNumber);
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

}
