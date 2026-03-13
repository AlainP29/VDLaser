using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Gcode.Models;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using VDLaser.Core.Services;
using VDLaser.ViewModels.Base;
using static VDLaser.Core.Gcode.Models.GcodeState;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Controller for manual machine piloting (Jogging), unit management, 
    /// user macros, and Laser preview.
    /// </summary>
    public partial class JoggingViewModel:ViewModelBase
    {
        #region Fields & Services
        private readonly ILogService _log;
        private readonly IGrblCoreService _coreService;
        private readonly IGcodeFileService _gcodeService;
        private readonly  IStatusPollingService _polling;
        private readonly GrblResponseParser _responseParser;
        private GrblState _grblState = new GrblState();
        private GcodeState _gcodeState = new GcodeState();
        #endregion

        #region Properties & Commands
        private const double ContinuousJogDistance = 500;
        private const double MmToInch = 1.0 / 25.4;
        private const double InchToMm = 25.4;
        private CancellationTokenSource _jogCts;

        private bool _isJoggingVMInitialized;
        private bool _isJoggingContinuous;
        #endregion

        #region Observables: Settings & Units
        [ObservableProperty]
        private bool _isLaserPreviewActive;
        [ObservableProperty]
        private string _laserPreviewStatusMessage= "Laser disabled.";
        [ObservableProperty]
        private double _manualFeedRate = 300;
        [ObservableProperty]
        private double _maxManualFeedRate = 2000;
        [ObservableProperty]
        private double _selectedStep = 1;
        [ObservableProperty]
        private string _tXLine = string.Empty;
        [ObservableProperty]
        private string _rXLine = string.Empty;
        [ObservableProperty]
        private string _feed = "0";
        [ObservableProperty]
        private string _speed = "0";
        [ObservableProperty]
        private bool _isSelectedKeyboard = false;
        [ObservableProperty]
        private bool _isSelectedMetric = true;
        [ObservableProperty]
        private bool _isLaserEnabled = false;
        [ObservableProperty]
        private string _macro1 = "G91 G0 X-25 Y-25 F2000";
        [ObservableProperty]
        private string _origine = "G90 G0 X0 Y0";
        [ObservableProperty]
        private string _macro2 = "G91 G1 X25 Y25 F500";
        [ObservableProperty]
        private string _macro3 = "G90 G1 X-50 Y-50";
        [ObservableProperty]
        private string _macro4 = "G91 G1 X-20 Y20 F1000";
        [ObservableProperty]
        private double _laserPower = 0;
        [ObservableProperty]
        private double _maxLaserPower = 2500;
        [ObservableProperty]
        private int _selectedLaser;
        [ObservableProperty]
        private RespStatus _responseStatus = RespStatus.Ok;
        #endregion

        #region Computed Properties
        public string FeedRateUnit => IsSelectedMetric ? "mm/min" : "in/min";
        public string DistanceUnit => IsSelectedMetric ? "mm" : "in";
        public bool IsJogEnabled => _coreService.IsConnected;
        public List<double> AvailableSteps { get; } = new() { 0.1, 0.5, 1, 5, 10 };
        #endregion

        public JoggingViewModel(IGrblCoreService coreService, ILogService log, IGcodeFileService gcodeService, IStatusPollingService polling)
        {
            _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _gcodeService = gcodeService ?? throw new ArgumentNullException(nameof(coreService));
            _polling= polling ?? throw new ArgumentNullException(nameof(polling));

            _responseParser = new GrblResponseParser(_log);

            _isJoggingVMInitialized = true;

            _coreService.DataReceived += OnGrblDataReceived;
            _coreService.PropertyChanged += OnCoreServicePropertyChanged;

            LogContextual(_log, "Initialized", "Jogging and Laser control ready");
        }

        #region event handlers
        partial void OnIsSelectedMetricChanging(bool value)
        {
            if (!_coreService.IsConnected)
            {
                LogContextual(_log, "Metric change ignored", "serial port closed");
                IsSelectedMetric = !value;
            }
        }
        partial void OnIsSelectedMetricChanged(bool value)
        {
            if (!_isJoggingVMInitialized || !_coreService.IsConnected)
                return;

            _coreService.SendCommandAsync(value ? "G21" : "G20");

            if (value)
            {
                ManualFeedRate = Math.Round(ManualFeedRate * InchToMm, 0);
                SelectedStep = 1.0;
                MaxManualFeedRate = 2000;
            }
            else
            {
                ManualFeedRate = Math.Round(ManualFeedRate * MmToInch, 1);
                SelectedStep = 0.1;
                MaxManualFeedRate = Math.Round(2000 * MmToInch, 1);
            }
            LogContextual(_log, "UnitsChanged", $"Switched to {(value ? "Metric" : "Imperial")}");
            NotifyUnitsChanged();
        }
        private void NotifyUnitsChanged()
        {
            OnPropertyChanged(nameof(FeedRateUnit));
            OnPropertyChanged(nameof(DistanceUnit));
            OnPropertyChanged(nameof(MaxManualFeedRate));
        }
        private void OnCoreServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IGrblCoreService.IsConnected))
            {
                SendManualCommand.NotifyCanExecuteChanged();
                SendOrigineCommand.NotifyCanExecuteChanged();
                SendMacro1Command.NotifyCanExecuteChanged();
                SendMacro2Command.NotifyCanExecuteChanged();
                SendMacro3Command.NotifyCanExecuteChanged();
                SendMacro4Command.NotifyCanExecuteChanged();
                JogCommand.NotifyCanExecuteChanged();
                JogUpStartCommand.NotifyCanExecuteChanged();
                JogStopCommand.NotifyCanExecuteChanged();
                JogDownStartCommand.NotifyCanExecuteChanged();
                JogLeftStartCommand.NotifyCanExecuteChanged();
                JogRightStartCommand.NotifyCanExecuteChanged();
                JogSWStartCommand.NotifyCanExecuteChanged();
                JogNWStartCommand.NotifyCanExecuteChanged();
                JogSEStartCommand.NotifyCanExecuteChanged();
                JogNEStartCommand.NotifyCanExecuteChanged();
                StartLaserPreviewCommand.NotifyCanExecuteChanged();
                StopLaserPreviewCommand.NotifyCanExecuteChanged();
                ToggleLaserCommand.NotifyCanExecuteChanged();
                IncreaseFeedRateCommand.NotifyCanExecuteChanged();
                DecreaseFeedRateCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(IsJogEnabled));
                _log.Debug("[JoggingViewModel] IsConnected changed - CanExecute updated");
            }
        }
        private void OnGrblDataReceived(object? sender, DataReceivedEventArgs e)
        {
            var status = e.Text.Trim();

            if (status.StartsWith("ALARM:"))
            {
                _log.Warning("[JoggingViewModel] Runtime alarm detected in jog - Forcing poll.");
                _polling.ForcePoll();
            }

            if (string.IsNullOrEmpty(status)) return;
            //_log.Information("[JoggingViewModel] Response state: {stat}", status);

            if (TryParseLaserMax(status))
                return;

            if (_responseParser.CanParse(status))
            {
                _responseParser.Parse(status, _grblState);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ResponseStatus = RespStatus.Ok;
                    _log.Information("[JoggingViewModel] Jog state: {resp}", ResponseStatus);
                    SendManualCommand.NotifyCanExecuteChanged();
                });
            }
            else
            {
                //_log.Warning("[JoggingViewModel] Unhandled response: {resp}", status);
                ResponseStatus = RespStatus.Ok;  // Ajout
            }
        }
        partial void OnIsSelectedKeyboardChanged(bool value)
        {
            if (value)
            {
                ManualFeedRate = 1000;
                _log.Information("[JoggingViewModel] Keyboard activated : Feed rate = 1000.");
            }
            else
            { 
                ManualFeedRate = 300; 
                _log.Information("[JoggingViewModel] Keyboard deactivated : Feed rate reset to 300."); 
            }
            JogUpStartCommand.NotifyCanExecuteChanged();
            JogDownStartCommand.NotifyCanExecuteChanged();
            JogLeftStartCommand.NotifyCanExecuteChanged();
            JogRightStartCommand.NotifyCanExecuteChanged();
        }
        partial void OnSelectedLaserChanged(int value)
        {
            LaserPower = value;  // Met à jour le Slider quand ComboBox change
            _log.Information("[Laser] Puissance présélectionnée : {Power}", value);

        }
        /// <summary>
        /// Slider feed rate changed (aucun envoi direct)
        /// Max FeedRate=2000
        /// Min FeedRate=0
        /// </summary>
        /// <param name="value"></param>
        partial void OnManualFeedRateChanged(double value)
        {
            var clamped = Math.Clamp(value, 0, MaxManualFeedRate);

            if (clamped != value)
            {
                ManualFeedRate = clamped;
                return;
            }

            _log.Information("[JoggingViewModel] Feedrate = {Feed}", ManualFeedRate);

            if (!_coreService.IsConnected)
                return;
        }
        #endregion

        #region Commands : Send Gcode & Macros
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendManual()
        {
            ResponseStatus = RespStatus.Q;  // Ajout
            try
            {
                await _coreService.SendCommandAsync(TXLine);
            }
            catch (Exception ex)
            {
                _log.Error("[JOG] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");  // Test
            }
            finally
            {
                SendManualCommand.NotifyCanExecuteChanged();
                TXLine = string.Empty;
                LogContextual(_log, "SendManual", $"TX {TXLine}");
            }
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendOrigine()
        {
            ResponseStatus = RespStatus.Q;
            try
            {
                await _coreService.SendCommandAsync(Origine);
                LogContextual(_log, "SendOrigine", $"TX {Origine}");
            }
            catch (Exception ex)
            {
                _log.Error("[JOG] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");
            }
            finally
            {
                SendMacro1Command.NotifyCanExecuteChanged();
                ResponseStatus = RespStatus.Ok;
            }
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendMacro1()
        {
            ResponseStatus = RespStatus.Q;
            try
            {
                await _coreService.SendCommandAsync(Macro1);
                LogContextual(_log, "SendMacro1", $"TX Macro {Macro1}");
            }
            catch (Exception ex)
            {
                _log.Error("[JOG] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");
            }
            finally
            {
                SendMacro1Command.NotifyCanExecuteChanged();
                ResponseStatus = RespStatus.Ok;
            }
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendMacro2()
        {
            ResponseStatus = RespStatus.Q;
            try
            {
                await _coreService.SendCommandAsync(Macro2);
                LogContextual(_log, "SendMacro2", $"TX Macro {Macro2}");
            }
            catch (Exception ex)
            {
                _log.Error("[JOG] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");
            }
            finally
            {
                SendMacro2Command.NotifyCanExecuteChanged();
                ResponseStatus = RespStatus.Ok;
            }

        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendMacro3()
        {
            ResponseStatus = RespStatus.Q;
            try
            {
                await _coreService.SendCommandAsync(Macro3);
                LogContextual(_log, "SendMacro3", $"TX Macro {Macro3}");
            }
            catch (Exception ex)
            {
                _log.Error("[JOG] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");
            }
            finally
            {
                SendMacro3Command.NotifyCanExecuteChanged();
                ResponseStatus = RespStatus.Ok;
            }

        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendMacro4()
        {
            ResponseStatus = RespStatus.Q;
            try
            {
                await _coreService.SendCommandAsync(Macro4);
                LogContextual(_log, "SendMacro4", $"TX Macro {Macro4}");
            }
            catch (Exception ex)
            {
                _log.Error("[JOG] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");
            }
            finally
            {
                SendMacro4Command.NotifyCanExecuteChanged();
                ResponseStatus = RespStatus.Ok;
            }

        }
        #endregion

        #region Logic : Move (Jogging)
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        public async Task Jog(string parameter)
        {
            double moveX = 0;
            double moveY = 0;
            double moveZ = 0;

            if (parameter.Contains("X")) moveX = ExtractDirection(parameter, "X") * SelectedStep;
            if (parameter.Contains("Y")) moveY = ExtractDirection(parameter, "Y") * SelectedStep;
            if (parameter.Contains("Z")) moveZ = ExtractDirection(parameter, "Z") * SelectedStep;

            string unitMode = IsSelectedMetric ? "G21" : "G20";
            StringBuilder sb = new StringBuilder($"$J=G91 {unitMode} ");

            if (moveX != 0) sb.AppendFormat(CultureInfo.InvariantCulture, "X{0:F3} ", moveX);
            if (moveY != 0) sb.AppendFormat(CultureInfo.InvariantCulture, "Y{0:F3} ", moveY);
            if (moveZ != 0) sb.AppendFormat(CultureInfo.InvariantCulture, "Z{0:F3} ", moveZ);

            sb.AppendFormat(CultureInfo.InvariantCulture, "F{0:F0}", ManualFeedRate);

            _log.Debug("[JOG] Incremental job: {Cmd}", sb.ToString());
            await _coreService.SendCommandAsync(sb.ToString());
        }
        public void EmergencyStopJogging()
        {
            _jogCts?.Cancel();
            _isJoggingContinuous = false;

            _coreService.SendRealtimeCommand(0x85);
            LogContextual(_log, "EmergencyStopJogging", "Continuous jog stop requested");
            if (IsLaserPreviewActive)
            {
                Task.Run(() => _coreService.SendCommandAsync("M5"));
            }

            ResponseStatus = RespStatus.Ok;
            SendManualCommand.NotifyCanExecuteChanged();
        }
        private async Task StartContinuousJogAsync(double xDir, double yDir)
        {
            if (!_coreService.IsConnected|| _isJoggingContinuous)
                return;

            if (_coreService.State.MachineState != GrblState.MachState.Idle)
            {
                _log.Warning("[JOG] Jog continuous ignored : Machine not Idle.");
                return;
            }

            _isJoggingContinuous = true;
            _jogCts = new CancellationTokenSource();

            var feed = Math.Clamp(ManualFeedRate, 0, MaxManualFeedRate);
            double x = xDir * ContinuousJogDistance;
            double y = yDir * ContinuousJogDistance;

            if (x == 0 && y == 0)
                return;
            string unitMode = IsSelectedMetric ? "G21" : "G20";
            string cmd = string.Format(
                CultureInfo.InvariantCulture,
                "$J=G91 {0} X{1:F3} Y{2:F3} F{3:F3}",
                unitMode, x, y, feed);

            LogContextual(_log, "StartContinuousJogAsync", $"Requesting continuous jog - {cmd}");

            ResponseStatus = RespStatus.Q;
            SendManualCommand.NotifyCanExecuteChanged();

            LogContextual(_log, "StartContinuousJogAsync", $"Continuous jog start requested - DirX={xDir}, DirY={yDir} - {cmd}");
            await _coreService.SendCommandAsync(cmd);

            await Task.Delay(Timeout.Infinite, _jogCts.Token).ContinueWith(_ => { });
        }
        private async Task StopContinuousJogAsync()
        {
            if (!_isJoggingContinuous)
                return;

            EmergencyStopJogging();
            LogContextual(_log, "StopContinuousJogAsync", "Continuous jog stop requested");
            await Task.CompletedTask;
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogStop()
        {
            await StopContinuousJogAsync();
        }
        
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogUpStart()
        {
            await StartContinuousJogAsync(0, 1);
        }
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogDownStart()
        {
            await StartContinuousJogAsync(0, -1);
        }
        
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogLeftStart()
        {
            await StartContinuousJogAsync(-1, 0);
        }
        
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogRightStart()
        {
            await StartContinuousJogAsync(1, 0);
        }
        
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogSWStart()
        {
            await StartContinuousJogAsync(-1, -1);
        }
        
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogNWStart()
        {
            await StartContinuousJogAsync(-1, 1);
        }
        
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogSEStart()
        {
            await StartContinuousJogAsync(1,-1);
        }
        
        [RelayCommand(CanExecute = nameof(CanJog))]
        private async Task JogNEStart()
        {
            await StartContinuousJogAsync(1, 1);
        }
        #endregion

        #region Logic : Laser Control
        /// <summary>
        /// Commande start laser preview
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteLaserPreview))]
        public async Task StartLaserPreview()
        {
            if (_isLaserPreviewActive)
                return;

            var power = Math.Clamp(LaserPower, 0, MaxLaserPower);

            if (power <= 0)
            return;

            _isLaserPreviewActive = true;
            LaserPreviewStatusMessage = "Preview in progress...";

            // Utilisation de M3 (ou M4) avec un mouvement nul pour forcer l'allumage en mode dynamique
            string cmd = string.Format(
                CultureInfo.InvariantCulture,
                "G91 G1 X0 Y0 F100 M3 S{0}",
                power);

            _log.Warning("[JOG] [LASER] - Preview Start S{Power}", power);

            await _coreService.SendCommandAsync(cmd);
        }
        /// <summary>
        /// Commande STOP Laser Preview
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task StopLaserPreview()
        {
            if (!_isLaserPreviewActive)
                return;

            _isLaserPreviewActive = false;
            LaserPreviewStatusMessage = "Laser enabled";
            _log.Warning("[JOG] [LASER] - Preview Stop");

            await _coreService.SendCommandAsync("M5");
        }

        /// <summary>
        /// Sécurisation laser preview
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteLaserPreview()
        {
            if(!_coreService.IsConnected)
                LaserPreviewStatusMessage = "Communication error. Check connection";
            if(LaserPower == 0)
                LaserPreviewStatusMessage = "Increase laser power before previewing";
            return _coreService.IsConnected
                && IsLaserEnabled
                   && LaserPower > 0
                   && !_isLaserPreviewActive;
        }
        /// <summary>
        /// Gérer l’activation / désactivation laser
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private void ToggleLaser()
        {
            _log.Warning("[JOG] [LASER] - Enabled = {State}", IsLaserEnabled);
            if (IsLaserEnabled)
            {
                LaserPreviewStatusMessage = "Laser enabled";
            }
            else
            {
                LaserPreviewStatusMessage = "Laser disabled";
            }
        }
        partial void OnLaserPowerChanged(double value)
        {
            if (!IsLaserEnabled)
                LaserPower = 0;
            StartLaserPreviewCommand.NotifyCanExecuteChanged();
        }
        partial void OnIsLaserEnabledChanged(bool value)
        {
            if (!value)
            {
                if (_coreService.IsConnected)
                {
                    _ = _coreService.SendCommandAsync("M5");
                }
                LaserPower = 0;
                LaserPreviewStatusMessage = "Laser disabled.";
            }


            StartLaserPreviewCommand.NotifyCanExecuteChanged();
        }
        private bool TryParseLaserMax(string line)
        {
            if (!line.StartsWith("$30="))
                return false;

            var valuePart = line.Substring(4);

            if (!double.TryParse(
                valuePart,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double max))
            {
                _log.Warning("[JOG] [LASER] - Invalid $30 format: {Line}", line);
                return true;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                MaxLaserPower = max;
                LogContextual(_log, "SyncSettings", $"Max Power updated to {max}");
                if (LaserPower > MaxLaserPower)
                    LaserPower = MaxLaserPower;
            });

            _log.Warning("[JOG] [LASER] - GRBL $30 synchronized → MaxLaserPower = {Max}", max);
            return true;
        }
        #endregion

        #region Helpers & Utilities
        private bool CanExecuteSendCommand()
        {
            bool canExecute = _coreService.IsConnected;
            return canExecute;
        }
        private bool CanJog()
        {
            bool canExecute = IsSelectedKeyboard && _coreService.IsConnected && _coreService.State.MachineState == GrblState.MachState.Idle;
            return canExecute;
        }
        private int ExtractDirection(string p, string axis)
        {
            int index = p.IndexOf(axis) + 1;
            if (p[index] == '-') return -1;
            return 1;
        }
        /// <summary>
        /// Increase the motion speed with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        [RelayCommand(CanExecute = nameof(CanJog))]
        private void IncreaseFeedRate(bool parameter)
        {
            ManualFeedRate += 10;
            _log.Information("[JOG]|F{0}", ManualFeedRate);
        }
        /// <summary>
        /// Decrease the motion speed with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        [RelayCommand(CanExecute = nameof(CanJog))]
        private void DecreaseFeedRate(bool parameter)
        {
            ManualFeedRate -= 10;
            _log.Information("[JOG]|F{0}", ManualFeedRate);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _coreService.DataReceived -= OnGrblDataReceived;
                _coreService.PropertyChanged -= OnCoreServicePropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}
