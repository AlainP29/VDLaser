using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.ComponentModel;
using System.IO;
using System.Windows;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Grbl.Errors;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;
using VDLaser.ViewModels.Controls;
using VDLaser.ViewModels.Plotter;
using static VDLaser.Core.Grbl.Models.GrblState;


namespace VDLaser.ViewModels.Main
{
    /// <summary>
    /// ViewModel principal gérant l'état global de l'application, 
    /// la coordination des services GRBL et la navigation entre les sous-vues.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogService _log;
        private readonly GrblSettingsViewModel _settingVM;
        private readonly IStatusPollingService _polling;
        private readonly ControleViewModel _controleVM;
        private readonly IGcodeJobService _gcodeJobService;
        private readonly IGrblCoreService _grblService;
        private readonly ISerialPortService _serialService;
        private readonly IDialogService _dialogService;
        
        private bool CanDisconnect() => IsConnected;
        private bool CanConnect() => !IsConnecting && !IsConnected;
        public bool CanEditSerialSettings => !IsConnected && !IsConnecting;
        private bool CanEmergencyStop() => IsConnected;
        private bool _isAttemptingReconnect = false;
        #region State Properties
        [ObservableProperty]
        private MachineUiState _uiState;
        [ObservableProperty]
        private MachineStateViewModel _machineStateVM;
        [ObservableProperty]
        private MachState _machineState=MachState.Idle;
        [ObservableProperty]
        private int _selectedTabIndex;
        [ObservableProperty]
        private string _connectionState = "Disconnected";

        [ObservableProperty]
        public bool _isLoadingGlobal = false;

        [ObservableProperty]
        public bool _isConnecting = false;

        [ObservableProperty]
        public bool _isConnected = false;

        [ObservableProperty]
        private bool _isLoading = false;
        [ObservableProperty]
        private bool _isFileViewVisible = true;

        [ObservableProperty]
        private bool _isPlotterViewVisible = false;

        [ObservableProperty]
        private bool _isSettingsViewVisible = false;
        [ObservableProperty]
        private bool _isJoggingViewVisible = false;
        [ObservableProperty]
        private string _currentTab;
        public event EventHandler<string>? ConnectionError;
        public SerialPortSettingViewModel SerialPortSettingVM { get; }
        public PlotterViewModel PlotterVM { get; }
        public LoggingSettingsViewModel LoggingSettingsVM { get; }
        public GcodeSettingsViewModel GcodeSettingsVM { get; }
        public bool IsIdle => MachineState == MachState.Idle;
        #endregion


        public MainWindowViewModel(
            IGrblCoreService grblService, 
            ISerialPortService serialService,
            SerialPortSettingViewModel serialPortSettingVM, 
            GrblSettingsViewModel settingVM,
            ControleViewModel controleVM,
            IStatusPollingService polling,
            ILogService log,
            PlotterViewModel plotterVM, 
            IGcodeJobService jobService, 
            IDialogService dialogService, 
            LoggingSettingsViewModel loggingSettingsVM,
            MachineStateViewModel machineStateVM,
            GcodeSettingsViewModel gcodeSettingsVM)
        {
            _grblService = grblService ?? throw new ArgumentNullException(nameof(grblService));
            _gcodeJobService=jobService?? throw new ArgumentNullException(nameof(jobService));
            _serialService=serialService?? throw new ArgumentNullException(nameof(serialService));
            _polling = polling ?? throw new ArgumentNullException(nameof(polling));
            SerialPortSettingVM = serialPortSettingVM ?? throw new ArgumentNullException(nameof(serialPortSettingVM));
            _settingVM = settingVM ?? throw new ArgumentNullException(nameof(settingVM));
            _controleVM = controleVM ?? throw new ArgumentNullException(nameof(controleVM));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _dialogService=dialogService??  throw new ArgumentNullException(nameof(dialogService));
            PlotterVM = plotterVM ?? throw new ArgumentNullException(nameof(plotterVM));
            LoggingSettingsVM = loggingSettingsVM ?? throw new ArgumentNullException(nameof(loggingSettingsVM));
            _machineStateVM = machineStateVM ?? throw new ArgumentNullException(nameof(machineStateVM));
            GcodeSettingsVM=gcodeSettingsVM?? throw new ArgumentNullException(nameof(gcodeSettingsVM));

            _log.Information("[Main] initialized and services injected.");

            CurrentTab = "Home";
            // Abonnements aux événements
            _settingVM.PropertyChanged += OnSettingVMPropertyChanged;
            _grblService.PropertyChanged += OnGrblServicePropertyChanged;
            _controleVM.PropertyChanged += OnControleVMPropertyChanged;

            _serialService.ConnectionLost += async (s, e) => {
                _log.Fatal("[Main] Lien USB perdu ! Arrêt du logiciel.");
                await EmergencyStop();
                await HandleAutoReconnect();
                _dialogService.ShowErrorAsync("Le câble USB semble avoir été débranché. Le job est annulé.");
            };
        }

        #region Events
        partial void OnIsConnectedChanged(bool value)
        {
            OnPropertyChanged(nameof(CanEditSerialSettings));
            EmergencyStopCommand.NotifyCanExecuteChanged();
        }
        partial void OnIsConnectingChanged(bool value)
        {
            OnPropertyChanged(nameof(CanEditSerialSettings));
        }
        private void OnSettingVMPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GrblSettingsViewModel.IsLoading))
            {
                IsLoading = _settingVM.IsLoading;  // Copie l'état local vers global
            }
        }
        /// <summary>
        /// To deactivate the UI Idle indicator when state changes.
        /// </summary>
        /// <param name="value"></param>
        partial void OnMachineStateChanged(MachState value)
        {
            OnPropertyChanged(nameof(IsIdle));
            if (value != MachState.Idle)
            {
                if (SelectedTabIndex is 2 or 4)
                    SelectedTabIndex = 3; // Plotter
            }
        }
        #endregion

        #region Logic : State Management
        /// <summary>
        /// Gère la transition des états de la machine en fonction du cycle de Homing.
        /// </summary>
        private void OnControleVMPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ControleViewModel.IsHomingInProgress) && _controleVM.IsHomingInProgress)
            {
                _log.Debug("[Main] Homing sequence started via ControleViewModel");
                UiState = MachineUiState.Homing;
            }

            if (e.PropertyName == nameof(ControleViewModel.IsHomingOk) && _controleVM.IsHomingOk)
            {
                if (_controleVM.IsHomingOk)
                {
                    _log.Information("[Main] Homing successful. Machine is now Ready.");
                    UiState = MachineUiState.Ready;
                }
            }
        }
        /// <summary>
        /// Intercepteur de changement d'état pour mettre à jour l'UI Textuelle.
        /// </summary>
        partial void OnUiStateChanged(MachineUiState value)
        {

            _log.Debug("[Main] Machine UI State transition to: {NewState}", value);

            ConnectionState = value switch
            {
                MachineUiState.Disconnected => "Disconnected",
                MachineUiState.Connecting => "Connecting...",
                MachineUiState.Connected => "Connected",
                MachineUiState.HomingRequired => "Home Required",
                MachineUiState.Homing => "Homing...",
                MachineUiState.Ready => "Ready",
                MachineUiState.EmergencyStop => "Emergency Stop - Re-homing required",
                MachineUiState.Alarm => "Alarm",
                _ => "Unknown"
            };
        }
        /// <summary>
        /// Réagit aux changements d'état profonds venant du service GRBL (Cœur de la logique).
        /// </summary>
        private void OnGrblServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IGrblCoreService.IsConnected))
                {
                _log.Information("[Main] Serial connection established. Starting Polling Service.");
                _polling.Start();
                
            }
            if (_grblService.State.MachineState == MachState.Alarm)
            {
                if (UiState != MachineUiState.Alarm)
                    _log.Warning("[Main] Hardware Alarm detected! Machine state locked.");

                UiState = MachineUiState.Alarm;
            }
            // Automate d'état basé sur le retour GRBL
            if (e.PropertyName == nameof(IGrblCoreService.State))
            {
                var grblRawState = _grblService.State.MachineState;

                // Cas 1 : Fin de connexion, attente de Homing
                if (grblRawState == MachState.Idle && IsLoadingGlobal &&
                    _grblService.HasLoadedSettings)
                {
                    //_log.Information("[MainWindowViewModel] Settings loaded. Machine requires Homing to unlock.");
                    UiState = MachineUiState.HomingRequired;
                }

                // Cas 2 : Homing en cours (détecté par GRBL)
                if (grblRawState == MachState.Home)
                {
                    UiState = MachineUiState.Homing;
                    IsLoadingGlobal = false;
                }

                // Cas 3 : Machine libre
                if (grblRawState == MachState.Idle && !IsLoadingGlobal)
                {
                    UiState = MachineUiState.Ready;
                }
            }
        }
        #endregion

        #region Commands : Connection / Disconnection
        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task Connect()
        {
            _log.Information("[Main] Connection - Attempting to connect to {Port}...", SerialPortSettingVM.PortName);

            IsConnecting = true;
            IsLoading = true;
            IsLoadingGlobal = true;
            UiState = MachineUiState.Connecting;
            try
            {
                SerialPortSettingVM.ApplySettings();
                await _grblService.ConnectAsync();

                IsConnected = true;
                UiState = MachineUiState.Connected;
                _log.Information("[Main] Connection - Successfully connected to GRBL controller.");
            }
            catch (GrblConnectionException ex)
            {
                _log.Warning("[Main] Connection {Error} - {Message}", ex.Error,ex.Message);
                IsConnected = false;
                ConnectionState = ex.Message;
                ConnectionError?.Invoke(this, ex.Message);
            }
            catch (TimeoutException)
            {
                IsConnected = false;
                ConnectionState = "No response (timeout)";
            }
            catch (IOException ex)
            {
                _log.Error("[Main] Connection - IO Error. Port {Port} might be in use or unavailable.", SerialPortSettingVM.PortName);
                IsConnected = false;
                ConnectionState = $"Impossible to open {SerialPortSettingVM.PortName}";
            }
            catch (Exception ex)
            {
                _log.Fatal("[Main] Connection - Unexpected error during connection attempt: {Message}", ex);
                IsConnected = false;
                ConnectionState = "Fatal Connection error";
            }
            finally
            {
                IsConnecting = false;
                IsLoading = false;

                NotifyCommandStates();
            }
        }

        [RelayCommand(CanExecute = nameof(CanDisconnect))]
        private async Task Disconnect()
        {
            _log.Information("[Main] Connection - User requested disconnection.");

            if (!IsConnected) return;

            IsLoading = true;
            try 
            { 
                await _grblService.DisconnectAsync();
                IsConnected = false;
                UiState = MachineUiState.Disconnected;
                ConnectionState = "Disconnected";
            }
            catch (Exception ex)
            {
                _log.Error("[Main] Connection - Error during clean disconnect.");
            }
            finally
            {
                IsLoading = false;
                NotifyCommandStates();
            }
        }
        #endregion

        #region Commands : Safety
        [RelayCommand(CanExecute = nameof(CanEmergencyStop))]
        public async Task EmergencyStop()
        {

            _log.Warning("[Main] Safety - EMERGENCY STOP TRIGGERED BY USER.");

            try
            {
                await _grblService.SendRealtimeCommandAsync((byte)'!');
                await Task.Delay(50);
                await _grblService.SendRealtimeCommandAsync(0x18);

                if (_gcodeJobService.IsRunning)
                {
                    _gcodeJobService.Stop();
                }

                await _grblService.SendCommandAsync("M5");

                UiState = MachineUiState.EmergencyStop;
                _log.Information("[Main] Safety - Emergency Stop sequence sent successfully.");
            }
            catch (Exception ex)
            {
                _log.Fatal("[Main] Safety - FAILED TO SEND EMERGENCY STOP COMMAND!");
            }
        }

        private async Task HandleAutoReconnect()
        {
            if (_isAttemptingReconnect) return;
            _isAttemptingReconnect = true;
            UiState = MachineUiState.Reconnecting;
            _log.Warning("[Main] Mode reconnexion automatique activé...");

            // On récupère le nom du port qui vient d'être perdu
            string lostPort = _serialService.PortName;

            while (IsConnected)
            {
                _log.Information("[Main] Tentative de reconnexion sur {Port}...", lostPort);

                // 1. Vérifier si le port est de nouveau visible dans Windows
                if (_serialService.IsPortAvailable(lostPort))
                {
                    try
                    {
                        await Task.Delay(500);
                        _serialService.Open();
                        if (IsConnected)
                        {
                            _log.Information("[Main] Reconnexion réussie !");
                            _isAttemptingReconnect = false;

                            // Optionnel : On peut tenter un Reset GRBL pour repartir propre
                            await _grblService.SendRealtimeCommandAsync(0x18);
                            return;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _log.Warning("[Main] Accès refusé à {Port} (encore utilisé par le système). Nouvelle tentative...", _serialService.PortName);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("[Main] Erreur lors de la réouverture : {Msg}", ex.Message);
                    }
                }

                // 2. Attendre 2 secondes avant la prochaine tentative
                await Task.Delay(2000);
            }

            _isAttemptingReconnect = false;
            UiState = MachineUiState.Connected;
        }

        [RelayCommand]
        private void SelectTab(string tabName)
        {
            CurrentTab = tabName;
            _log.Debug("[Main] Navigation vers l'onglet : {Tab}", tabName);
        }
        private void OnGrblErrorReceived(object? sender, int errorCode)
        {
            if (errorCode != 0)
            {
                // On force l'affichage de l'onglet G-Code pour montrer l'erreur
                CurrentTab = "File";

                _dialogService.ShowErrorAsync($"Erreur GRBL détectée : {errorCode}", "Erreur de Commande");
            }
        }
        #endregion

        #region Helpers
        private void NotifyCommandStates()
        {
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
            EmergencyStopCommand.NotifyCanExecuteChanged();
        }
        [RelayCommand]
        private void ShowView(string viewName)
        {
            // On réinitialise tout à false
            IsJoggingViewVisible = false;
            IsFileViewVisible = false;
            IsPlotterViewVisible = false;
            IsSettingsViewVisible = false;
            

            // On active la vue demandée
            switch (viewName)
            {
                case "Jogging": IsJoggingViewVisible = true; break;
                case "File": IsFileViewVisible = true; break;
                case "Plotter": IsPlotterViewVisible = true; break;
                case "Settings": IsSettingsViewVisible = true; break;
            }

            _log.Debug("[Main] Switched view to: {ViewName}", viewName);
        }
        [RelayCommand]
        private async Task QuitApplication()
        {
            bool confirm = await _dialogService.AskConfirmationAsync(
        "Do you want to quit the application?",
        "Exit VDLaser");

            if (!confirm) return;

            try
            {
                if (_gcodeJobService.IsRunning)
                {
                    _log.Warning("[Main] Quit - A job is currently running. Stopping job before exit.");
                    _gcodeJobService.Stop();
                }
                if (_grblService.IsConnected)
                {
                    _grblService.SendCommandAsync("M5").Wait(500);
                    await _grblService.DisconnectAsync();
                }
                _log.Information("[Main] Application exit requested by user.");
            }
            catch (Exception ex)
            {
                _log.Error("[Main] Error during application exit: {Message}", ex);
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
                System.Windows.Application.Current.Shutdown();
            }
            
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _log.Debug("[MainWindowViewModel] Disposing ViewModel and cleaning events.");
                _settingVM.PropertyChanged -= OnSettingVMPropertyChanged;
                _grblService.PropertyChanged -= OnGrblServicePropertyChanged;
                _controleVM.PropertyChanged -= OnControleVMPropertyChanged;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
