using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Gère les commandes de contrôle direct de la machine (Homing, Jog, Reset, Alarme).
    /// </summary>
    public partial class ControleViewModel : ViewModelBase
    {
        #region Fields & Properties
        private readonly ILogService _log;
        private readonly IGrblCoreService _coreService;
        private readonly IStatusPollingService _polling;

        [ObservableProperty]
        private bool _isHomingInProgress = false;
        [ObservableProperty]
        public bool _isHomingOk = false; // Exemple de propriété pour activer/désactiver la commande
        [ObservableProperty]
        public bool _isKillAlarmAvailable = true; // Propriété pour activer/désactiver la commande KillAlarm
        [ObservableProperty]
        private string _initializationStatus = "Initializing...";
        [ObservableProperty]
        private bool _isPaused;
        [ObservableProperty]
        private string _localizedToolTip = string.Empty;
        #endregion
        public ControleViewModel(IGrblCoreService coreService, 
            ILogService log,IStatusPollingService polling)
        {
            _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _polling = polling ?? throw new ArgumentNullException(nameof(polling));
            _log.Information("[ControleViewModel] Initialized");

            _coreService.PropertyChanged += OnCoreServicePropertyChanged;
            _coreService.StatusUpdated += OnMachineStatusUpdated;
        }
        #region Logic : Service Events
        private void OnCoreServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IGrblCoreService.IsConnected))
            {
                _log.Debug("[ControleViewModel] Connection state changed: {IsConnected}", _coreService.IsConnected);
                RefreshAllCommands();
                IsHomingInProgress = false;
            }
        }
        private void RefreshAllCommands()
        {
            HomingCommand.NotifyCanExecuteChanged();
            KillAlarmCommand.NotifyCanExecuteChanged();
            SoftResetCommand.NotifyCanExecuteChanged();
            StartCycleCommand.NotifyCanExecuteChanged();
            FeedHoldCommand.NotifyCanExecuteChanged();
            Home1Command.NotifyCanExecuteChanged();
            Home2Command.NotifyCanExecuteChanged();
            SetWorkZeroCommand.NotifyCanExecuteChanged();
        }
        private void OnMachineStatusUpdated(object? sender, EventArgs e)
        {
            // On récupère l'état actuel depuis le service
            // GrblCoreService met à jour son objet State avant de déclencher l'événement
            var currentState = _coreService.State;

            if (currentState != null)
            {
                // On met à jour IsPaused si la machine est en mode "Hold"
                // Cela déclenchera automatiquement l'animation dans le XAML
                IsPaused = (currentState.MachineState == GrblState.MachState.Hold);

                // Debug optionnel pour vérifier la réception dans la console de sortie
                // _log.Debug($"[Controle] Status Updated: {currentState.MachineState} (IsPaused: {IsPaused})");
            }
        }
        #endregion

        #region Commands : Machine Motion (Homing / Jog)
        [RelayCommand(CanExecute = nameof(CanHoming))]
        private async Task Homing()
        {
            _log.Information("[ControleViewModel] Action - User triggered Homing ($H).");
            IsHomingInProgress = true;
            

            try
            {
                await _coreService.SendCommandAsync("$H");
            }
            catch (Exception ex)
            {
                _log.Error("[ControleViewModel] Action - Failed to send Homing command", ex);
                IsHomingInProgress = false;
            }
            finally
            {

                HomingCommand.NotifyCanExecuteChanged();
            }
        }
        [RelayCommand(CanExecute = nameof(CanExecuteRealTimeCommand))]
        public async Task SetWorkZeroAsync()
        {
            if (!_coreService.IsConnected) return;

            _log.Information("[Action] Setting Work Zero (G10 L20 P1 X0 Y0).");
            try
            {
                await _coreService.SendCommandAsync("G10 L20 P1 X0 Y0");

                _log.Information("[ControleViewModel] Action - Work Zero successfully defined.");
            }
            catch (Exception ex)
            {
                _log.Error("[ControleViewModel] Action - Failed to set Work Zero.");
            }
        }
        #endregion

        #region Commands : Real-Time & Safety
        /// <summary>
        /// Write Grbl '$X' command to kill alarm mode. In real-time command and canexecuterealTimeCommand in order to kill alarm
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanKillAlarm))]
        private async Task KillAlarm()
        {
            _log.Warning("[ControleViewModel] Safety - User attempting to Kill Alarm ($X).");
            IsKillAlarmAvailable = false;
            try
            {
                await UnlockAlarmAsync();
            }
            catch
            {
                _log.Error("[ControleViewModel] Safety - Failed to send kill alarm command");
            }
            finally
            {
                IsKillAlarmAvailable = true;
                KillAlarmCommand.NotifyCanExecuteChanged();
                await Task.Delay(500);
                await _coreService.SendRealtimeCommandAsync((byte)'?');
            }
        }

        /// <summary>
        /// Writes Grbl real-time command (asci dec 24) or 0x18 (Ctrl-x) for a soft reset
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteRealTimeCommand))]
        private async Task SoftReset()
        {
            _log.Information("[ControleViewModel] Safety - User triggered Soft Reset (0x18).");
            try
            {
                await SoftResetAsync();
            }
            catch
            {
                _log.Error("[ControleViewModel] Safety - Failed to send soft reset command");
            }
            finally
            {
                SoftResetCommand.NotifyCanExecuteChanged();
            }
        }
        /// <summary>
        /// Writes Grbl "~" real-time command (ascii dec 126) to start or resume the machine after a pause or 'M0' command.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteRealTimeCommand))]
        private async Task StartCycle()
        {
            _log.Information("[ControleViewModel] RealTime - Start/Resume (~) sent.");
            await SendRealtimeWithLog(126, "Start/Resume");
        }
        /// <summary>
        /// Writes Grbl "!" real-time command (ascii dec 33) to pause the machine motion X, Y and Z (not spindle or laser).
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteRealTimeCommand))]
        private async Task FeedHold()
        {
            _log.Debug("[ControleViewModel] RealTime - Feed Hold (!) sent.");
            await SendRealtimeWithLog(33, "Feed Hold");
        }
        #endregion

        #region Internal Helpers
        private async Task SendRealtimeWithLog(byte command, string label)
        {
            try
            { 
                await _coreService.SendRealtimeCommandAsync(command);
            }
            catch (Exception ex)
            { 
                _log.Error("[ControleViewModel] RealTime - Failed to send {Label}", label); 
            }
        }
        /// <summary>
        /// Write Grbl 'G28' command to go to the pre-defined position 1 set by G28.1 command.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanHoming))]
        private async Task Home1()
        {
            _log.Information("[ControleViewModel] Action - User triggered pre-defined 1 command G28 sent");
            try
            {
                await _coreService.SendCommandAsync("G28");
            }
            catch
            {
                _log.Error("[ControleViewModel] Failed to send pre-defined 1 G28 command");
            }
            finally
            {
                Home1Command.NotifyCanExecuteChanged();
            }
        }
        /// <summary>
        /// Write Grbl 'G30' command to go to the pre-defined position 2 set by G30.1 command.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanHoming))]
        private async Task Home2()
        {
            _log.Information("[ControleViewModel] Action - User triggered pre-defined 2 command G30.");
            try
            {
                await _coreService.SendCommandAsync("G30");
            }
            catch
            {
                _log.Error("[ControleViewModel] Action - Failed to send pre-defined 2 G30 command");
            }
            finally
            {
                Home2Command.NotifyCanExecuteChanged();
            }
        }
        private async Task UnlockAlarmAsync()
        {
            var result = MessageBox.Show(
                "Attention : La commande $X (Kill Alarm) déverrouille l'alarme sans résoudre la cause. " +
                "Cela peut entraîner une perte de position de la machine et des risques de sécurité. " +
                "Voulez-vous continuer ? Assurez-vous de faire un homing ($H) ensuite."+
                "Soft-Reset peut-être nécessaire avant la commande $X pour acquiter l'alarme",
                "Avertissement - Déverrouillage Alarme",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _log.Information("[ControleViewModel] Safety - User confirmed Alarm Unlock ($X).");
                await _coreService.SendCommandAsync("$X");
                await _coreService.SendRealtimeCommandAsync((byte)'?');
                await Task.Delay(200); // Attends réponse
                _polling.ForcePoll(); // Reset pending pour retenter immédiatement
                _log.Information("[ControleViewModel] Forced poll after $X.");
            }
            else
            {
                _log.Information("[ControleViewModel] Safety - Alarm Unlock cancelled by user.");
            }
        }
        private async Task SoftResetAsync()
        {
            var result = MessageBox.Show(
                "Attention : La commande Soft Reset (0x18) réinitialise GRBL et arrête tous les mouvements en cours. " +
                "Cela peut entraîner une perte de position si la machine est en opération. " +
                "Voulez-vous continuer ? Pensez à vérifier l'état de la machine ensuite.",
                "Avertissement - Soft Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                _log.Information("[ControleViewModel] Safety - User confirmed Soft Reset (0x18).");
                //await _coreService.SendRealtimeCommandAsync(24);
                await _coreService.SendRealtimeCommandAsync(0x18);
            }
            else
            {
                _log.Information("[ControleViewModel] Safety - Commande Soft Reset canceled.");
            }
        }
        #endregion
        /// <summary>
        /// Allows/disallows real-time command. These commands can be sent at anytime,
        /// anywhere, and Grbl will immediately respond, no matter what it's doing
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteRealTimeCommand() => _coreService.IsConnected;
        private bool CanKillAlarm() => IsKillAlarmAvailable && _coreService.IsConnected;
        private bool CanHoming() => _coreService.IsConnected;// && _coreService.HasLoadedSettings; button is always disabled
        private bool CanExecuteGCodeCommand() => (_coreService.IsConnected && IsHomingOk);// && _coreService.HasLoadedSettings;
        
        public void Dispose()
        {
            _log.Debug("[Controle] Disposing ControleViewModel.");
            _coreService.PropertyChanged -= OnCoreServicePropertyChanged;
            _coreService.StatusUpdated -= OnMachineStatusUpdated;
        }
    }
}
