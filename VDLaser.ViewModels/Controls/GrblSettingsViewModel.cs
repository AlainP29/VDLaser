using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Handles GRBL configuration, status querying, and hardware information display.
    /// </summary>
    public partial class GrblSettingsViewModel : ViewModelBase
    {
        #region Fields & Services
        private readonly ILogService _log;
        private readonly IGrblCoreService _coreService;
        private readonly GrblSettingsParser _settingsParser;
        private readonly GrblInfoParser _infoParser;
        private readonly IGrblCommandQueue _commandQueue;
        private readonly SemaphoreSlim _loadLock = new(1, 1);
        public GcodeFileViewModel GcodeFileVM { get; }
        #endregion

        #region Properties
        private GrblState _grblState = new GrblState();
        private GrblInfo _grblInfo = new GrblInfo();

        [ObservableProperty]
        private string _grblVersion = string.Empty;

        [ObservableProperty]
        private string _grblBuild = string.Empty;

        [ObservableProperty]
        private string _compileOptions = string.Empty;

        [ObservableProperty]
        private string _blockBufferSize = string.Empty;

        [ObservableProperty]
        private string _rxBufferSize = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private ObservableCollection<GrblSetting> _settings = new ObservableCollection<GrblSetting>();
        #endregion

        public GrblSettingsViewModel(IGrblCoreService coreService, ILogService log, IGrblCommandQueue commandQueue, GcodeFileViewModel gcodeFileVM)
        {
            _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _settingsParser = new GrblSettingsParser(_log);
            _infoParser = new GrblInfoParser(_log);
            _commandQueue = commandQueue ?? throw new ArgumentNullException(nameof(log));
            GcodeFileVM = gcodeFileVM ?? throw new ArgumentNullException(nameof(gcodeFileVM));

            _coreService.DataReceived += OnGrblDataReceived;
            _coreService.PropertyChanged += OnCoreServicePropertyChanged;
            _coreService.SettingsUpdated += OnSettingsUpdated;

            _log.Information("[SETTINGS] Initialized (Queued enabled");
        }
        /// <summary>
        /// Gestion des données reçues du GRBL. Parse seulement les lignes de settings.
        /// </summary>
        private void OnGrblDataReceived(object? sender, DataReceivedEventArgs e)
        {
            var line = e.Text;
            if (string.IsNullOrEmpty(line)) return;

            if (_infoParser.CanParse(line))
            {
                _infoParser.Parse(line, _grblInfo);
                // Marshal vers UI et met à jour les propriétés observables
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GrblVersion = _grblInfo.GrblVersion ?? string.Empty;
                    GrblBuild = _grblInfo.GrblBuild ?? string.Empty;
                    CompileOptions = _grblInfo.CompileOptions ?? string.Empty;
                    BlockBufferSize = _grblInfo.BlockBufferSize ?? string.Empty;
                    RxBufferSize = _grblInfo.RxBufferSize ?? string.Empty;
                    _log.Information("[SettingViewModel] GRBL Info parsed: Version={Version}, Build={Build}, Options={Options}", GrblVersion, GrblBuild, CompileOptions + "," + BlockBufferSize + "," + RxBufferSize);
                });
            }
            else if (_settingsParser.CanParse(line))
            {
                _settingsParser.Parse(line, _grblState);
                var parsedSetting = _grblState.LastParsedSetting;
                if (parsedSetting != null)
                {
                    // Marshal vers le thread UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Settings.Add(parsedSetting);
                        _log.Information("[SettingViewModel] Setting added: {Setting}", parsedSetting.ToString());
                    });
                }
            }
        }
        private void OnCoreServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IGrblCoreService.IsConnected))
                return;

            if (_coreService.IsConnected && !_coreService.HasLoadedSettings)
            {
                _log.Information("[SettingViewModel] Début chargement unique des settings sur connexion");
            }
            NotifyAllCommands();
        }

        private void NotifyAllCommands()
        {
            ResetXCommand.NotifyCanExecuteChanged();
            ResetYCommand.NotifyCanExecuteChanged();
            SetHome1Command.NotifyCanExecuteChanged();
            SetHome2Command.NotifyCanExecuteChanged();
            HelpCommand.NotifyCanExecuteChanged();
            CheckCommand.NotifyCanExecuteChanged();
            KillAlarmCommand.NotifyCanExecuteChanged();
            SleepCommand.NotifyCanExecuteChanged();
            StartupBlocksCommand.NotifyCanExecuteChanged();
            BuildInfoCommand.NotifyCanExecuteChanged();
            ParserStateCommand.NotifyCanExecuteChanged();
            ParametersCommand.NotifyCanExecuteChanged();
            CurrentStatusCommand.NotifyCanExecuteChanged();
            LoadSettingsCommand.NotifyCanExecuteChanged();
            LoadInfoCommand.NotifyCanExecuteChanged();
            LoadLaserCommand.NotifyCanExecuteChanged();

        }

        private void OnSettingsUpdated(object? sender, IReadOnlyCollection<GrblSetting> settings)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Option 1 : Remplacer toute la collection (efficace si invoqué souvent)
                Settings = new ObservableCollection<GrblSetting>(settings);

                // Option 2 : Ajouter seulement les nouveaux (pour éviter duplicates)
                // foreach (var setting in settings)
                // {
                //     if (!Settings.Any(s => s.Id == setting.Id))
                //     {
                //         Settings.Add(setting);
                //         _log.Information("[SettingViewModel] Setting added: {Setting}", setting.ToString());
                //     }
                // }
            });
        }

        #region Commands
        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task LoadSettingsAsync()
        {
            if (!await _loadLock.WaitAsync(0))
            {
                LogContextual(_log, "LoadSettingsAsync", "Settings already loaded, ignored");

                return;
            }
            try
            {
                IsLoading = true;
                Settings.Clear();

                //await _commandQueue.EnqueueAsync("$I", "Settings");
                await _commandQueue.EnqueueAsync("$$", "Settings");
                _coreService.MarkSettingsLoaded();
                LogContextual(_log, "LoadSettingsAsync", "Settings loaded once");
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }
        private async Task LoadGrblInfoAsync()
        {
            if (_coreService.HasLoadedSettings) 
            {
                LogContextual(_log, "LoadGrblInfoAsync", "Info GRBL already loaded, ignored");

                return;
            }

            await _commandQueue.EnqueueAsync("$I", "Settings");
        }
        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task LoadInfo() =>
              LoadGrblInfoAsync();
        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task LoadLaser()
        {
            return Task.CompletedTask; 
        }
        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task ResetX()
        {
            LogContextual(_log, "ResetX", "Reset axis X");
            await _commandQueue.EnqueueAsync("$H", "Reset");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task ResetY()
        {
            LogContextual(_log, "ResetY", "Reset axis Y");
            await _commandQueue.EnqueueAsync("$H", "Reset");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task SetHome1()
        {
            LogContextual(_log, "SetHome1", "Set pre-define home 1 position (G28.1)");
            await _commandQueue.EnqueueAsync("G28.1", "Home");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task SetHome2()
        {
            LogContextual(_log, "SetHome2", "Set pre-define home 2 position (G30.1)");
            await _commandQueue.EnqueueAsync("G30.1", "Home");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGeneralCommand))]
        private async Task Help()
        {
            LogContextual(_log, "Help", "Requesting help ($)");
            await _commandQueue.EnqueueAsync("$", "Help");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGeneralCommand))]
        private async Task Sleep()
        {
            LogContextual(_log, "Sleep", "Requesting system sleep ($SLP)");
            await _commandQueue.EnqueueAsync("$SLP", "System");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task Check()
        {
            LogContextual(_log, "Check", "Requesting system Check ($C)");
            await _commandQueue.EnqueueAsync("$C", "System");
        }
        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task KillAlarm() =>
            _commandQueue.EnqueueAsync("$X", "System");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task StartupBlocks()
        {
            LogContextual(_log, "StartupBlocks", "Requesting system startup blocks ($N)");
            await _commandQueue.EnqueueAsync("$N", "System");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task BuildInfo()
        {
            LogContextual(_log, "BuildInfo", "Querying firmware build information");
            await LoadGrblInfoAsync();
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task ParserState()
        {
            LogContextual(_log, "ParserState", "Querying G-Code parser state ($G)");
            await _commandQueue.EnqueueAsync("$G", "Parser");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task Parameters()
        {
            LogContextual(_log, "Parameters", "Querying GRBL parameters ($#)");
            await _commandQueue.EnqueueAsync("$#", "Parser");
        }

        /// <summary>
        /// Initiates a real-time status uses raw byte 63 ('?') to trigger an immediate status response from the GRBL controller, 
        /// bypassing the normal command queue. This allows for quick updates of the machine's current state without waiting for queued commands to execute.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExecuteRealTimeCommand))]
        private Task CurrentStatus() =>
            _coreService.SendRealtimeCommandAsync(63);
        #endregion

        #region Predicates
        private bool CanExecuteRealTimeCommand() => _coreService.IsConnected;
        private bool CanExecuteGrblCommand() => _coreService.IsConnected && !IsLoading && _coreService.HasLoadedSettings;
        private bool CanExecuteGeneralCommand() => _coreService.IsConnected && !IsLoading;
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _coreService.PropertyChanged -= OnCoreServicePropertyChanged;
                _coreService.DataReceived -= OnGrblDataReceived;
                _coreService.SettingsUpdated -= OnSettingsUpdated;
                _loadLock.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
