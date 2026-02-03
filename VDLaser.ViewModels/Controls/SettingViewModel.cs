using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using VDLaser.Core.Gcode;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    public partial class SettingViewModel : ViewModelBase
    {
        private readonly ILogService _log;
        private readonly IGrblCoreService _coreService;
        private readonly GrblSettingsParser _settingsParser;
        private readonly GrblInfoParser _infoParser;
        private readonly IGrblCommandQueue _commandQueue;
        public GcodeFileViewModel GcodeFileVM { get; }
        private GrblState _grblState = new GrblState();
        private GrblInfo _grblInfo = new GrblInfo();
        private readonly SemaphoreSlim _loadLock = new(1, 1);

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

        public SettingViewModel(IGrblCoreService coreService, ILogService log, IGrblCommandQueue commandQueue, GcodeFileViewModel gcodeFileVM)
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
            _log.Information("[SettingViewModel] Initialised (Queued enabled");
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
        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private async Task LoadSettingsAsync()
        {
            if (!await _loadLock.WaitAsync(0))
            {
                _log.Information("[SettingViewModel] LoadSettings déjà en cours, ignoré");
                return;
            }
            try
            {
                IsLoading = true;
                Settings.Clear();

                //await _commandQueue.EnqueueAsync("$I", "Settings");
                await _commandQueue.EnqueueAsync("$$", "Settings");
                _coreService.MarkSettingsLoaded();
                _log.Information("[SettingViewModel] Settings loaded once");
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }
        private async Task LoadGrblInfoAsync()
        {
            if (_coreService.HasLoadedSettings) // Déjà chargé, ignore ou affiche cached
            {
                _log.Information("[SettingViewModel] Info GRBL déjà chargée, ignoré");
                return;
            }

            await _commandQueue.EnqueueAsync("$I", "Settings");
            // Optionnel : Marquer loaded si c'est le seul appel
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
        private Task ResetX() =>
            _commandQueue.EnqueueAsync("$H", "Reset");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task ResetY() =>
            _commandQueue.EnqueueAsync("$H", "Reset");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task SetHome1() =>
            _commandQueue.EnqueueAsync("G28.1", "Home");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task SetHome2() =>
            _commandQueue.EnqueueAsync("G30.1", "Home");

        [RelayCommand(CanExecute = nameof(CanExecuteGeneralCommand))]
        private Task Help() =>
            _commandQueue.EnqueueAsync("$", "Help");

        [RelayCommand(CanExecute = nameof(CanExecuteGeneralCommand))]
        private Task Sleep() =>
            _commandQueue.EnqueueAsync("$SLP", "System");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task Check() =>
            _commandQueue.EnqueueAsync("$C", "System");
        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task KillAlarm() =>
            _commandQueue.EnqueueAsync("$X", "System");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task StartupBlocks() =>
            _commandQueue.EnqueueAsync("$N", "System");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task BuildInfo() =>
            LoadGrblInfoAsync();

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task ParserState() =>
            _commandQueue.EnqueueAsync("$G", "Parser");

        [RelayCommand(CanExecute = nameof(CanExecuteGrblCommand))]
        private Task Parameters() =>
            _commandQueue.EnqueueAsync("$#", "Parser");

        // Realtime : reste direct (GRBL spec)
        [RelayCommand(CanExecute = nameof(CanExecuteRealTimeCommand))]
        private Task CurrentStatus() =>
            _coreService.SendRealtimeCommandAsync(63);

        private bool CanExecuteRealTimeCommand() => _coreService.IsConnected;
        private bool CanExecuteGrblCommand() => _coreService.IsConnected && !IsLoading && _coreService.HasLoadedSettings;
        private bool CanExecuteGeneralCommand() => _coreService.IsConnected && !IsLoading;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _coreService.PropertyChanged -= OnCoreServicePropertyChanged;
                _coreService.DataReceived -= OnGrblDataReceived;
                _coreService.SettingsUpdated -= OnSettingsUpdated;
            }
            base.Dispose(disposing);
        }
    }
}
