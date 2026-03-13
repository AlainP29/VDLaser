using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.IO;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Grbl.Errors;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;
using VDLaser.ViewModels.Controls;
using VDLaser.ViewModels.Enums;
using VDLaser.ViewModels.Plotter;
using static VDLaser.Core.Grbl.Models.GrblState;


namespace VDLaser.ViewModels.Main
{
    /// <summary>
    /// Core ViewModel managing the global application state, 
    /// coordinating GRBL services, and handling main navigation.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region Fields & Services
        private readonly ILogService _log;
        private readonly IStatusPollingService _polling;
        private readonly IGcodeJobService _gcodeJobService;
        private readonly IGrblCoreService _grblService;
        private readonly ISerialPortService _serialService;
        private readonly IDialogService _dialogService;

        private readonly GrblSettingsViewModel _settingVM;
        private readonly ControleViewModel _controleVM;
        public SerialPortSettingViewModel SerialPortSettingVM { get; }
        public PlotterViewModel PlotterVM { get; }
        public LoggingSettingsViewModel LoggingSettingsVM { get; }
        public GcodeSettingsViewModel GcodeSettingsVM { get; }

        #endregion

        #region Observables State Properties
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
        #endregion

        #region Computed Properties
        public bool IsIdle => MachineState == MachState.Idle;
        public bool CanEditSerialSettings => !IsConnected && !IsConnecting;
        private bool _isAttemptingReconnect = false;
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
            _settingVM = settingVM ?? throw new ArgumentNullException(nameof(settingVM));
            _controleVM = controleVM ?? throw new ArgumentNullException(nameof(controleVM));
            _machineStateVM = machineStateVM ?? throw new ArgumentNullException(nameof(machineStateVM));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _dialogService=dialogService??  throw new ArgumentNullException(nameof(dialogService));

            SerialPortSettingVM = serialPortSettingVM ?? throw new ArgumentNullException(nameof(serialPortSettingVM));
            PlotterVM = plotterVM ?? throw new ArgumentNullException(nameof(plotterVM));
            LoggingSettingsVM = loggingSettingsVM ?? throw new ArgumentNullException(nameof(loggingSettingsVM));
            GcodeSettingsVM=gcodeSettingsVM?? throw new ArgumentNullException(nameof(gcodeSettingsVM));

            CurrentTab = "Home";

            _settingVM.PropertyChanged += OnSettingVMPropertyChanged;
            _grblService.PropertyChanged += OnGrblServicePropertyChanged;
            _controleVM.PropertyChanged += OnControleVMPropertyChanged;
            _serialService.ConnectionLost += OnConnectionLost;

            LogContextual(_log, "Initialized", "Main Application Shell ready");
        }

        #region Events
        public event EventHandler<string>? ConnectionError;
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
        partial void OnMachineStateChanged(MachState value)
        {
            OnPropertyChanged(nameof(IsIdle));
            if (value != MachState.Idle)
            {
                if (SelectedTabIndex is 2 or 4)
                    SelectedTabIndex = 3;
            }
        }
        private void OnGrblErrorReceived(object? sender, int errorCode)
        {
            if (errorCode != 0)
            {
                CurrentTab = "File";

                _dialogService.ShowErrorAsync($"Error GRBL détected : {errorCode}", "Command error");
            }
        }
        #endregion

        #region Logic : State Management
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
                    LogContextual(_log, "Main", "Homing successful. Machine is now Ready.");

                    UiState = MachineUiState.Ready;
                }
            }
        }
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
        private void OnGrblServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IGrblCoreService.IsConnected))
                {
                LogContextual(_log, "Main", "Serial connection established. Starting Polling Service.");

                _polling.Start();
                
            }
            if (_grblService.State.MachineState == MachState.Alarm)
            {
                if (UiState != MachineUiState.Alarm)
                    _log.Warning("[Main] Hardware Alarm detected! Machine state locked.");

                UiState = MachineUiState.Alarm;
            }
            if (e.PropertyName == nameof(IGrblCoreService.State))
            {
                var grblRawState = _grblService.State.MachineState;

                if (grblRawState == MachState.Idle && IsLoadingGlobal &&
                    _grblService.HasLoadedSettings)
                {
                    LogContextual(_log, "Main", "Settings loaded. Machine requires Homing to unlock.");

                    UiState = MachineUiState.HomingRequired;
                }

                if (grblRawState == MachState.Home)
                {
                    UiState = MachineUiState.Homing;
                    IsLoadingGlobal = false;
                }

                if (grblRawState == MachState.Idle && !IsLoadingGlobal)
                {
                    UiState = MachineUiState.Ready;
                }
            }
        }
        #endregion

        #region Commands : Connection / Disconnection
        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task ConnectAsync()
        {
            LogContextual(_log, "ConnectAsync", $"Attempting to connect to {SerialPortSettingVM.PortName}");

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
                LogContextual(_log, "ConnectAsync", "Connection - Successfully connected to GRBL controller.");
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
        private async Task DisconnectAsync()
        {
            LogContextual(_log, "DisconnectAsync", "Connection - User requested disconnection.");

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
        private bool CanDisconnect() => IsConnected;
        private bool CanConnect() => !IsConnecting && !IsConnected;
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
                LogContextual(_log, "EmergencyStop", "Safety - Emergency Stop sequence sent successfully.");
            }
            catch (Exception ex)
            {
                _log.Fatal("[Main] Safety - FAILED TO SEND EMERGENCY STOP COMMAND!");
            }
        }
        /// <summary>
        /// Handles critical USB disconnection. 
        /// Triggers emergency procedures and user notification.
        /// </summary>
        private async void OnConnectionLost(object? sender, EventArgs e)
        {
            _log.Fatal("[Main] Lien USB perdu ! Arrêt du logiciel.");

            try
            {
                await EmergencyStop();

                _ = _dialogService.ShowErrorAsync("Connection Lost",
                    "The USB cable seems to be disconnected. The current job has been cancelled.");

                await HandleAutoReconnect();
            }
            catch (Exception ex)
            {
                _log.Error("[Main] Error during connection loss handling: {Message}", ex);
            }
        }
        private async Task HandleAutoReconnect()
        {
            if (_isAttemptingReconnect) return;
            _isAttemptingReconnect = true;
            UiState = MachineUiState.Reconnecting;
            _log.Warning("[Main] Mode automatic reconnection actived...");

            string lostPort = _serialService.PortName;

            while (IsConnected)
            {
                LogContextual(_log, "HandleAutoReconnect", $"Attempting reconnection on {lostPort}");

                if (_serialService.IsPortAvailable(lostPort))
                {
                    try
                    {
                        await Task.Delay(500);
                        _serialService.Open();
                        if (IsConnected)
                        {
                            _log.Information("[Main] Reconnexion réussie !");
                            LogContextual(_log, "HandleAutoReconnect", "Success!");

                            _isAttemptingReconnect = false;

                            await _grblService.SendRealtimeCommandAsync(0x18);
                            return;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _log.Warning("[Main] Access denied to {Port} (still in use by the system). Trying again...", _serialService.PortName);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("[Main] Error during reconnection : {Msg}", ex.Message);
                    }
                }

                await Task.Delay(2000);
            }

            _isAttemptingReconnect = false;
            UiState = MachineUiState.Connected;
        }
        private bool CanEmergencyStop() => IsConnected;
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
            IsJoggingViewVisible = false;
            IsFileViewVisible = false;
            IsPlotterViewVisible = false;
            IsSettingsViewVisible = false;
            
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
        private void SelectTab(string tabName)
        {
            CurrentTab = tabName;
            _log.Debug("[Main] Navigation to tab: {Tab}", tabName);
        }
        [RelayCommand]
        private async Task QuitApplication()
        {
            bool confirm = await _dialogService.AskConfirmationAsync(
        "Do you want to quit the application?",
        "Exit VDLaser");

            if (!confirm) return;

            LogContextual(_log, "QuitApplication", "Starting shutdown sequence...");

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
                LogContextual(_log, "QuitApplication", "Application exit requested by user.");
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
        #endregion

        #region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _settingVM.PropertyChanged -= OnSettingVMPropertyChanged;
                _grblService.PropertyChanged -= OnGrblServicePropertyChanged;
                _controleVM.PropertyChanged -= OnControleVMPropertyChanged;
                _serialService.ConnectionLost -= OnConnectionLost;
                LogContextual(_log, "Disposed", "MainWindow resources released");
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
