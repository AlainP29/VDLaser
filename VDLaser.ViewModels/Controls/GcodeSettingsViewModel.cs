using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using VDLaser.Core.Gcode;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Manages G-Code processing settings and error handling configurations.
    /// </summary>
    public partial class GcodeSettingsViewModel : ViewModelBase
    {
        #region Fields & Services
        private readonly IGcodeJobService _gcodeJobService;
        private readonly ILogService _log;
        #endregion

        #region Properties
        [ObservableProperty]
        private GcodeErrorHandlingMode _selectedErrorMode;
        public bool IsJobNotRunning=>!_gcodeJobService.IsRunning;
        public IEnumerable<GcodeErrorHandlingMode> ErrorModeValues
            => Enum.GetValues(typeof(GcodeErrorHandlingMode)).Cast<GcodeErrorHandlingMode>();
        #endregion

        public GcodeSettingsViewModel(IGcodeJobService gcodeJobService, ILogService log)
        {
            _gcodeJobService = gcodeJobService;
            _log = log;

            _selectedErrorMode = GcodeErrorHandlingMode.Strict;
            _gcodeJobService.ErrorHandlingMode = GcodeErrorHandlingMode.Strict;

            _gcodeJobService.StateChanged += OnJobStateChanged;
            _log.Debug("[GCODESETTINGS] Initialized with Mode: {Mode}", _selectedErrorMode);

        }

        #region Event Handlers

        partial void OnSelectedErrorModeChanged(GcodeErrorHandlingMode value)
        {
            if (_gcodeJobService.IsRunning)
            {
                _log.Warning("[GCODESETTINGS] Configuration change ignored: Job is currently running.");
                SelectedErrorMode = _gcodeJobService.ErrorHandlingMode;
                return;
            }
            _gcodeJobService.ErrorHandlingMode = value;

            LogContextual(_log, "ErrorModeChanged", $"New Mode: {value}");
            
            WeakReferenceMessenger.Default.Send(new ErrorModeChangedMessage(value)); // Ajout pour notifier
        }

        private void OnJobStateChanged(object? sender, EventArgs e)
        {
            
                OnPropertyChanged(nameof(IsJobNotRunning));
        }
        #endregion

        #region Messages

        public record ErrorModeChangedMessage(GcodeErrorHandlingMode Mode);
        #endregion


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gcodeJobService.StateChanged -= OnJobStateChanged;
            }
            base.Dispose(disposing);
        }
    }
}
