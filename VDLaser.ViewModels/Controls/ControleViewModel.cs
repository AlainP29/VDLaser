using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Manages direct machine control commands (Homing, Jog, Reset, Alarm).
    /// </summary>
    public partial class ControleViewModel : ViewModelBase, IDisposable
    {
        #region Fields & Dependencies
        private readonly ILogService _log;
        private readonly IGrblCoreService _coreService;
        private readonly IStatusPollingService _polling;
        #endregion

        #region Observable Properties
        [ObservableProperty]
        private bool _isHomingInProgress = false;
        [ObservableProperty]
        public bool _isHomingOk = false;
        [ObservableProperty]
        public bool _isAlarmActive = false;
        [ObservableProperty]
        public bool _isKillAlarmAvailable = true;
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

            _log.Information("[CONTROLE] Initialized");

            _log.ProfileChanged += OnProfileChanged;
            _coreService.PropertyChanged += OnCoreServicePropertyChanged;
            _coreService.StatusUpdated += OnMachineStatusUpdated;
        }

        #region Events Handlers
        private void OnProfileChanged(object? sender, LogProfile newProfile)
        {
            Application.Current.Dispatcher.Invoke(() => RefreshAllCommands());
        }
        private void OnCoreServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IGrblCoreService.IsConnected))
            {
                _log.Debug("[CONTROLE] Connection state changed: {IsConnected}", _coreService.IsConnected);
                RefreshAllCommands();
                IsHomingInProgress = false;
            }
        }
        private void OnMachineStatusUpdated(object? sender, EventArgs e)
        {
            var currentState = _coreService.State;
            if (currentState == null) return;

            IsPaused = (currentState.MachineState == GrblState.MachState.Hold);

            IsAlarmActive = currentState.MachineState == GrblState.MachState.Alarm;

            if (IsHomingInProgress && currentState.MachineState == GrblState.MachState.Idle)
            {
                IsHomingOk = true;
                IsHomingInProgress = false;
                _log.Information("[CONTROLE] Homing sequence completed successfully. Machine is now referenced.");
            }
            else if (IsAlarmActive)
            {
                if (IsHomingOk)
                {
                    _log.Warning("[CONTROLE] Alarm detected: Homing status invalidated.");
                    IsHomingOk = false;
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                RefreshAllCommands();
            });
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
        #endregion

        #region Commands - Motion
        /// <summary>
        /// Executes the Homing cycle ($H).
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanHoming))]
        private async Task Homing()
        {
            LogContextual(_log, "Homing", "User triggered $H cycle");
            IsHomingInProgress = true;
            try
            {
                await _coreService.SendCommandAsync("$H");
            }
            catch (Exception ex)
            {
                _log.Error("[CONTROLE] Action - Failed to send Homing command", ex.Message);
                IsHomingInProgress = false;
            }
            finally
            {
                HomingCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Defines the current position as Work Zero (G10 L20 P1 X0 Y0).
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteGCodeCommand))]
        public async Task SetWorkZeroAsync()
        {
            if (!_coreService.IsConnected) return;

            LogContextual(_log, "SetWorkZero", "G10 L20 P1 X0 Y0");
            try
            {
                await _coreService.SendCommandAsync("G10 L20 P1 X0 Y0");
            }
            catch (Exception ex)
            {
                _log.Error("[CONTROLE] Action - Failed to set Work Zero.");
            }
        }
        /// <summary>
        /// Write Grbl 'G28' command to go to the pre-defined position 1 set by G28.1 command.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteGCodeCommand))]
        private async Task Home1()
        {
            LogContextual(_log, "Home1", "User triggered pre-defined 1 command G28 sent");
            try
            {
                await _coreService.SendCommandAsync("G28");
            }
            catch
            {
                _log.Error("[CONTROLE] Failed to send pre-defined 1 G28 command");
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
        [RelayCommand(CanExecute = nameof(CanExecuteGCodeCommand))]
        private async Task Home2()
        {
            LogContextual(_log, "Home2", "User triggered pre-defined 2 command G30.");
            try
            {
                await _coreService.SendCommandAsync("G30");
            }
            catch
            {
                _log.Error("[CONTROLE] Action - Failed to send pre-defined 2 G30 command");
            }
            finally
            {
                Home2Command.NotifyCanExecuteChanged();
            }
        }

        #endregion

        #region Commands - Real-Time & Safety
        /// <summary>
        /// Write Grbl '$X' command to kill alarm mode. In real-time command and canexecuterealTimeCommand in order to kill alarm
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanKillAlarm))]
        private async Task KillAlarm()
        {
            LogContextual(_log, "KillAlarm", "User attempting to Kill Alarm ($X).");
            IsKillAlarmAvailable = false;
            try
            {
                await UnlockAlarmAsync();
            }
            catch
            {
                _log.Error("[CONTROLE] Safety - Failed to send kill alarm command");
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
            LogContextual(_log, "SoftReset", "User attempting Soft Reset (0x18).");
            try
            {
                await SoftResetAsync();
            }
            catch
            {
                _log.Error("[CONTROLE] Safety - Failed to send soft reset command");
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
        private async Task StartCycle() => await SendRealtimeWithLog(126, "Cycle Start/Resume (~)");
        /// <summary>
        /// Writes Grbl "!" real-time command (ascii dec 33) to pause the machine motion X, Y and Z (not spindle or laser).
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteRealTimeCommand))]
        private async Task FeedHold() => await SendRealtimeWithLog(33, "Feed Hold (!)");
        #endregion

        #region Dynamic Access Control (Modes & Safety)
        /// <summary>
        /// Allows/disallows real-time command. These commands can be sent at anytime,
        /// anywhere, and Grbl will immediately respond, no matter what it's doing.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteRealTimeCommand() => _coreService.IsConnected;
        /// <summary>
        /// Critical safety: $X (Kill Alarm) is primarily a SUPPORT/MAINTENANCE tool.
        /// </summary>
        /// <returns></returns>
        private bool CanKillAlarm()
        {
            return IsKillAlarmAvailable && _coreService.IsConnected;
        }
        private bool CanHoming() => _coreService.IsConnected && !IsHomingInProgress;
        /// <summary>
        /// Determines if G-Code movement commands (Home 1/2, Set Zero) are allowed.
        /// </summary>
        private bool CanExecuteGCodeCommand()
        {
            if (!_coreService.IsConnected) return false;

            if (_log.CurrentProfile == LogProfile.Support) return true;

            return IsHomingOk;
        }
        #endregion

        #region Internal Helpers
        private async Task SendRealtimeWithLog(byte command, string label)
        {
            try
            {
                _log.Debug("[CONTROLE] Sending Real-time: {Label} (0x{Cmd:X2})", label, command);
                await _coreService.SendRealtimeCommandAsync(command);
            }
            catch (Exception ex)
            { 
                _log.Error("[CONTROLE] RealTime - Failed to send {Label}", label); 
            }
        }
        private async Task UnlockAlarmAsync()
        {
            var result = MessageBox.Show(
                "Attention : La commande $X (Kill Alarm) déverrouille l'alarme sans résoudre la cause. " +
                "Cela peut entraîner une perte de position de la machine et des risques de sécurité. " +
                "Voulez-vous continuer ? Assurez-vous de faire un homing ($H) ensuite." +
                "Soft-Reset peut-être nécessaire avant la commande $X pour acquiter l'alarme",
                "Avertissement - Déverrouillage Alarme",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _coreService.SendCommandAsync("$X");
                await _coreService.SendRealtimeCommandAsync((byte)'?');
                await Task.Delay(200);
                _polling.ForcePoll();
                _log.Information("[ControleVM] Safety - User confirmed Alarm Unlock ($X) and forced poll after $X.");
            }
            else
            {
                _log.Information("[ControleVM] Safety - Alarm Unlock cancelled by user.");
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
                await _coreService.SendRealtimeCommandAsync(0x18);
                _log.Information("[ControleVM] Safety - User confirmed Soft Reset (0x18).");
            }
            else
            {
                _log.Information("[ControleVM] Safety - Soft Reset canceled.");
            }
        }
        #endregion

        public void Dispose()
        {
            _log.Debug("[CONTROLE] Disposing resources.");
            _log.ProfileChanged -= OnProfileChanged;
            _coreService.PropertyChanged -= OnCoreServicePropertyChanged;
            _coreService.StatusUpdated -= OnMachineStatusUpdated;
            GC.SuppressFinalize(this);
        }
    }
}
