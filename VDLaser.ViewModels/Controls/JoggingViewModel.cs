using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
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
    /// Contrôleur pour le pilotage manuel (Jogging), la gestion des unités, 
    /// les macros utilisateur et la prévisualisation Laser.
    /// </summary>
    public partial class JoggingViewModel:ViewModelBase
    {
        #region Fields & Properties
        private readonly ILogService _log;
        private readonly IGrblCoreService _coreService;
        private readonly IGcodeFileService _gcodeService;
        private readonly  IStatusPollingService _polling; // Injecter via le constructeur si nécessaire
        private readonly GrblResponseParser _responseParser;
        private readonly KeyboardShortcutManager _shortcutManager;
        private GrblState _grblState = new GrblState();
        private GcodeState _gcodeState = new GcodeState();

        // Constantes de mouvement
        private const double ContinuousJogDistance = 500;// Distance théorique pour simuler un mouvement fluide
        private const double MmToInch = 1.0 / 25.4;
        private const double InchToMm = 25.4;
        private CancellationTokenSource _jogCts;  // Pour annulation manuelle/timeout

        private bool _isJoggingVMInitialized;
        private bool _isJoggingContinuous;
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
        private string _tXLine = string.Empty;// Commande manuelle saisie
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
        // Configuration des Macros
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
        public string FeedRateUnit => IsSelectedMetric ? "mm/min" : "in/min";
        public string DistanceUnit => IsSelectedMetric ? "mm" : "in";
        public bool IsJogEnabled => _coreService.IsConnected;
        public List<double> AvailableSteps { get; } = new() { 0.1, 0.5, 1, 5, 10 };
        [ObservableProperty]
        private RespStatus _responseStatus = RespStatus.Ok;
        #endregion
        public JoggingViewModel(IGrblCoreService coreService, ILogService log, IGcodeFileService gcodeService, IStatusPollingService polling)
        {
            _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _gcodeService = gcodeService ?? throw new ArgumentNullException(nameof(coreService));
            _polling= polling ?? throw new ArgumentNullException(nameof(polling));
            _responseParser = new GrblResponseParser(_log);
            _shortcutManager = new KeyboardShortcutManager();

            _isJoggingVMInitialized = true;
            _coreService.DataReceived += OnGrblDataReceived;
            _coreService.PropertyChanged += OnCoreServicePropertyChanged;

            _log.Debug("[JoggingViewModel] Initialized.");
        }
        #region Logic : Gestion des Unités & États
        partial void OnIsSelectedMetricChanging(bool value)
        {
            if (!_coreService.IsConnected)
            {
                _log.Warning("[JoggingViewModel] Metric change ignored: serial port closed");
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
            _log.Information("[JoggingViewModel] Units changed : {Unit}", value ? "Métrique (mm)" : "Impérial (in)");
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
                // Notifier toutes les commandes dépendant de IsConnected
                SendManualCommand.NotifyCanExecuteChanged();
                SendOrigineCommand.NotifyCanExecuteChanged();
                SendMacro1Command.NotifyCanExecuteChanged();
                SendMacro2Command.NotifyCanExecuteChanged();
                SendMacro3Command.NotifyCanExecuteChanged();
                SendMacro4Command.NotifyCanExecuteChanged();
                JogCommand.NotifyCanExecuteChanged();
                JogUpStartCommand.NotifyCanExecuteChanged();
                JogUpStopCommand.NotifyCanExecuteChanged();
                JogDownStartCommand.NotifyCanExecuteChanged();
                JogDownStopCommand.NotifyCanExecuteChanged();
                JogLeftStartCommand.NotifyCanExecuteChanged();
                JogLeftStopCommand.NotifyCanExecuteChanged();
                JogRightStartCommand.NotifyCanExecuteChanged();
                JogRightStopCommand.NotifyCanExecuteChanged();
                JogSWStartCommand.NotifyCanExecuteChanged();
                JogSWStopCommand.NotifyCanExecuteChanged();
                JogNWStartCommand.NotifyCanExecuteChanged();
                JogNWStopCommand.NotifyCanExecuteChanged();
                JogSEStartCommand.NotifyCanExecuteChanged();
                JogSEStopCommand.NotifyCanExecuteChanged();
                JogNEStartCommand.NotifyCanExecuteChanged();
                JogNEStopCommand.NotifyCanExecuteChanged();
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
                _polling.ForcePoll(); // Inject IStatusPollingService _polling via ctor
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
       
        partial void OnSelectedLaserChanged(int value)
        {
            LaserPower = value;  // Met à jour le Slider quand ComboBox change
            _log.Information("[Laser] Puissance présélectionnée : {Power}", value);
        }
        // Dans votre logique de réception des réglages ($$) de la machine
        private void OnSettingsReceived()
        {
            // Si $110 est la vitesse max X
            if (_coreService.State.Settings.TryGetValue(110, out var maxSpeed))
            {
                //MaxManualFeedRate = double.Parse(maxSpeed, CultureInfo.InvariantCulture);
            }
        }
        #endregion

        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendManual()
        {
            ResponseStatus = RespStatus.Q;  // Ajout
            try
            {
                await _coreService.SendCommandAsync(TXLine);
                _log.Information("[JoggingViewModel] TX {cmd}", TXLine);
            }
            catch (Exception ex)
            {
                _log.Error("[JoggingViewModel] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");  // Test
            }
            finally
            {
                SendManualCommand.NotifyCanExecuteChanged();
                TXLine = string.Empty;
                _log.Information("[JoggingViewModel] TX {cmd}", TXLine);
                //ResponseStatus= RespStatus.Ok;  // Ajout
            }
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task SendOrigine()
        {
            ResponseStatus = RespStatus.Q;
            try
            {
                await _coreService.SendCommandAsync(Origine);
                _log.Information("[JoggingViewModel] TX Macro {cmd}", Origine);
            }
            catch (Exception ex)
            {
                _log.Error("[JoggingViewModel] Failed to load info: {Message}", ex.Message);
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
                _log.Information("[JoggingViewModel] TX Macro {cmd}", Macro1);
            }
            catch (Exception ex)
            {
                _log.Error("[JoggingViewModel] Failed to load info: {Message}", ex.Message);
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
                _log.Information("[JoggingViewModel] TX Macro {cmd}", Macro2);
            }
            catch (Exception ex)
            {
                _log.Error("[JoggingViewModel] Failed to load info: {Message}", ex.Message);
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
                _log.Information("[JoggingViewModel] TX Macro {cmd}", Macro3);
            }
            catch (Exception ex)
            {
                _log.Error("[JoggingViewModel] Failed to load info: {Message}", ex.Message);
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
                _log.Information("[JoggingViewModel] TX Macro {cmd}", Macro4);
            }
            catch (Exception ex)
            {
                _log.Error("[JoggingViewModel] Failed to load info: {Message}", ex.Message);
                MessageBox.Show($"Erreur : {ex.Message}");
            }
            finally
            {
                SendMacro4Command.NotifyCanExecuteChanged();
                ResponseStatus = RespStatus.Ok;
            }

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

            _log.Information("[Jogging] Feedrate = {Feed}", ManualFeedRate);

            if (!_coreService.IsConnected)
                return;
        }

        #region Logic : Mouvements (Jogging)
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        public async Task Jog(string parameter)
        {
            // On initialise les déplacements à 0
            double moveX = 0;
            double moveY = 0;
            double moveZ = 0;

            // Analyse du paramètre (ex: "X1Y1", "X-1Y1", "Z1")
            if (parameter.Contains("X")) moveX = ExtractDirection(parameter, "X") * SelectedStep;
            if (parameter.Contains("Y")) moveY = ExtractDirection(parameter, "Y") * SelectedStep;
            if (parameter.Contains("Z")) moveZ = ExtractDirection(parameter, "Z") * SelectedStep;
            // Utilisation de G21 (mm) ou G20 (pouces) selon la propriété

            string unitMode = IsSelectedMetric ? "G21" : "G20";
            StringBuilder sb = new StringBuilder($"$J=G91 {unitMode} ");

            if (moveX != 0) sb.AppendFormat(CultureInfo.InvariantCulture, "X{0:F3} ", moveX);
            if (moveY != 0) sb.AppendFormat(CultureInfo.InvariantCulture, "Y{0:F3} ", moveY);
            if (moveZ != 0) sb.AppendFormat(CultureInfo.InvariantCulture, "Z{0:F3} ", moveZ);

            sb.AppendFormat(CultureInfo.InvariantCulture, "F{0:F0}", ManualFeedRate);

            _log.Debug("[JoggingViewModel] Jog incrémental : {Cmd}", sb.ToString());
            await _coreService.SendCommandAsync(sb.ToString());
        }
        
        /// <summary>
        /// Méthode centrale : démarrer le jogging continu
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private async Task StartContinuousJogAsync(double xDir, double yDir)
        {
            if (!_coreService.IsConnected|| _isJoggingContinuous)
                return;

            if (_coreService.State.MachineState != GrblState.MachState.Idle)
            {
                _log.Warning("[JoggingViewModel] Jog continu ignoré : Machine pas en Idle.");
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

            _log.Information("[JoggingViewModel] START {Cmd}", cmd);

            ResponseStatus = RespStatus.Q;
            SendManualCommand.NotifyCanExecuteChanged();

            _log.Information("[JoggingViewModel] START continu — DirX={XDir}, DirY={YDir}, Cmd={Cmd}", xDir, yDir, cmd);
            System.Diagnostics.Debug.WriteLine($"[Debug VDLaser] Commande GRBL envoyée : {cmd}");  // Visible dans Output Window VS

            await _coreService.SendCommandAsync(cmd);

            await Task.Delay(Timeout.Infinite, _jogCts.Token).ContinueWith(_ => { });
        }
        /// <summary>
        /// Arrêt immédiat du jogging
        /// </summary>
        /// <returns></returns>
        private async Task StopContinuousJogAsync()
        {
            if (!_isJoggingContinuous)
                return;

            _jogCts?.Cancel();
            _isJoggingContinuous = false;

            await _coreService.SendRealtimeCommandAsync(0x85);// Jog Cancel GRBL 1.1
            _log.Information("[JoggingViewModel] STOP continu demandé");

            if (IsLaserPreviewActive) await _coreService.SendCommandAsync("M5");

            ResponseStatus = RespStatus.Ok;
            SendManualCommand.NotifyCanExecuteChanged();
        }
        /// <summary>
        /// Commandes clavier (KeyDown / KeyUp)
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogUpStart()
        {
            await StartContinuousJogAsync(0, 1);
        }

        [RelayCommand]
        private async Task JogUpStop()
        {
            await StopContinuousJogAsync();
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogDownStart()
        {
            await StartContinuousJogAsync(0, -1);
        }

        [RelayCommand]
        private async Task JogDownStop()
        {
            await StopContinuousJogAsync();
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogLeftStart()
        {
            await StartContinuousJogAsync(-1, 0);
        }

        [RelayCommand]
        private async Task JogLeftStop()
        {
            await StopContinuousJogAsync();
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogRightStart()
        {
            await StartContinuousJogAsync(1, 0);
        }

        [RelayCommand]
        private async Task JogRightStop()
        {
            await StopContinuousJogAsync();
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogSWStart()
        {
            await StartContinuousJogAsync(-1, -1);
        }
        [RelayCommand]
        private async Task JogSWStop()
        {
            await StopContinuousJogAsync();
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogNWStart()
        {
            await StartContinuousJogAsync(-1, 1);
        }
        [RelayCommand]
        private async Task JogNWStop()
        {
            await StopContinuousJogAsync();
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogSEStart()
        {
            await StartContinuousJogAsync(1,-1);
        }
        [RelayCommand]
        private async Task JogSEStop()
        {
            await StopContinuousJogAsync();
        }
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private async Task JogNEStart()
        {
            await StartContinuousJogAsync(1, 1);
        }
        [RelayCommand]
        private async Task JogNEStop()
        {
            await StopContinuousJogAsync();
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

            _log.Warning("[JoggingViewModel] LASER - Preview Start S{Power}", power);

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
            _log.Warning("[JoggingViewModel] LASER - Preview Stop");

            // Laser OFF
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
            _log.Warning("[JoggingViewModel] LASER - Enabled = {State}", IsLaserEnabled);
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
                _log.Warning("[JoggingViewModel] LASER - Invalid $30 format: {Line}", line);
                return true;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                MaxLaserPower = max;

                if (LaserPower > MaxLaserPower)
                    LaserPower = MaxLaserPower;
            });

            _log.Warning("[JoggingViewModel] LASER - GRBL $30 synchronized → MaxLaserPower = {Max}", max);
            return true;
        }
        #endregion

        /// <summary>
        /// Increase the motion speed with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        [RelayCommand(CanExecute = nameof(CanExecuteLaserCommand))]
        private void IncreaseFeedRate(bool parameter)
        {
            ManualFeedRate += 10;
            _log.Information("[JoggingViewModel]|F{0}", ManualFeedRate);
        }
        /// <summary>
        /// Decrease the motion speed with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        [RelayCommand(CanExecute = nameof(CanExecuteSendCommand))]
        private void DecreaseFeedRate(bool parameter)
        {
            ManualFeedRate -= 10;
            _log.Information("[JoggingViewModel]|F{0}", ManualFeedRate);
        }
        private bool CanExecuteSendCommand()
        {
            bool canExecute = _coreService.IsConnected;// && ResponseStatus == RespStatus.Ok;
            return canExecute;
        }
        private bool CanExecuteLaserCommand()
        {
            bool canExecute = _coreService.IsConnected;
            return canExecute;
        }
        // Utilitaire pour extraire la direction (1 ou -1) après la lettre de l'axe
        private int ExtractDirection(string p, string axis)
        {
            int index = p.IndexOf(axis) + 1;
            if (p[index] == '-') return -1;
            return 1;
        }
        public void HandleKeyPress(Key key)
        {
            _shortcutManager.HandleKeyPress(key);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _coreService.DataReceived -= OnGrblDataReceived;
                _coreService.PropertyChanged -= OnCoreServicePropertyChanged;
                _log.Debug("[JoggingViewModel] Libération des ressources.");
            }
            base.Dispose(disposing);
        }
    }
}
