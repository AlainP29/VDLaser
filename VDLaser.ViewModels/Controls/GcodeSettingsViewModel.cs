using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using VDLaser.Core.Gcode;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    public partial class GcodeSettingsViewModel : ViewModelBase
    {
        private readonly IGcodeJobService _gcodeJobService;
        private readonly ILogService _log;

        [ObservableProperty]
        private GcodeErrorHandlingMode _selectedErrorMode;
        public bool IsJobNotRunning=>!_gcodeJobService.IsRunning;
        public GcodeSettingsViewModel(IGcodeJobService gcodeJobService, ILogService log)
        {
            _gcodeJobService = gcodeJobService;
            _log = log;

            _selectedErrorMode = GcodeErrorHandlingMode.Strict;
            _gcodeJobService.ErrorHandlingMode = GcodeErrorHandlingMode.Strict;
            _log.Debug("[CONFIG] GcodeSettingsViewModel synchronisé : Mode={Mode}", _selectedErrorMode);

            _gcodeJobService.StateChanged += OnJobStateChanged;
        }

        partial void OnSelectedErrorModeChanged(GcodeErrorHandlingMode value)
        {
            if (_gcodeJobService.IsRunning)
            {
                _log.Warning("[CONFIG] Changement de mode ignoré car un job G-code est en cours d'exécution.");
                // Optionnel : Rétablir la valeur actuelle dans l'UI pour éviter confusion
                SelectedErrorMode = _gcodeJobService.ErrorHandlingMode;
                return;
            }
            _gcodeJobService.ErrorHandlingMode = value;
            _log.Information("[CONFIG] Mode de gestion des erreurs changé en : {Mode}", value);
            WeakReferenceMessenger.Default.Send(new ErrorModeChangedMessage(value)); // Ajout pour notifier
        }
        public record ErrorModeChangedMessage(GcodeErrorHandlingMode Mode);

        private void OnJobStateChanged(object? sender, EventArgs e)
        {
            
                OnPropertyChanged(nameof(IsJobNotRunning));
        }
        public IEnumerable<GcodeErrorHandlingMode> ErrorModeValues
            => Enum.GetValues(typeof(GcodeErrorHandlingMode)).Cast<GcodeErrorHandlingMode>();
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
