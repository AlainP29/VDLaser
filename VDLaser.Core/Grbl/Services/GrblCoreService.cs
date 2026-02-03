using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Windows.Controls;
using VDLaser.Core.Grbl.Errors;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Core.Grbl.Services
{
    // <summary>
    /// Service central de gestion de la communication avec le contrôleur GRBL.
    /// Gère le cycle de vie de la connexion, le parsing des retours et l'état de la machine.
    /// </summary>
    public class GrblCoreService : IGrblCoreService, IDisposable, INotifyPropertyChanged
    {
        #region Fields & Properties
        private readonly ISerialPortService _config;
        private readonly ILogService _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IGrblSubParser> _parsers;

        private ISerialConnection? _serial;
        private readonly GrblState _state = new();
        private readonly GrblInfo _grblInfo = new();

        
        public event EventHandler<DataReceivedEventArgs>? DataReceived;
        public event EventHandler? StatusUpdated;
        public event EventHandler<IReadOnlyCollection<GrblSetting>>? SettingsUpdated;
        public event EventHandler<GrblInfo>? InfoUpdated;
        public event PropertyChangedEventHandler? PropertyChanged; // Ajout pour notifications
        
        public event EventHandler<bool>? ConnectionStateChanged;
        public event EventHandler? StatusLineReceived;

        public bool IsConnected => _serial?.IsOpen == true;
        public GrblState State => _state;
        public GrblInfo GrblInfo => _grblInfo;

        public bool IsLaserPower { get; private set; } = false;
        private bool _hasLoadedSettings = false;
        public bool HasLoadedSettings
        {
            get => _hasLoadedSettings;
            private set
            {
                _hasLoadedSettings = value;
                OnPropertyChanged(nameof(HasLoadedSettings));
            }
        }
        private TaskCompletionSource<bool>? _grblHandshakeTcs;
        private readonly GrblRxRingBuffer _rxRingBuffer = new();

        #endregion

        public GrblCoreService(ISerialPortService config, ILogService log,IEnumerable<IGrblSubParser> parsers,IServiceProvider serviceProvider,ISerialConnection? serialOverride = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _parsers = parsers?.ToList() ?? throw new ArgumentNullException(nameof(parsers));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serial = serialOverride;//injection mock pour tests unitaires

            _log.Information("[GrblCoreService] Initialized");
        }

        
        private void InitializeSerialPort()
        {
            if (string.IsNullOrWhiteSpace(_config.PortName))
                throw new InvalidOperationException("Port COM not defined.");

            var port = new SerialPort
            {
                PortName = _config.PortName,
                BaudRate = _config.BaudRate,
                Parity = _config.Parity,
                DataBits = _config.DataBits,
                StopBits = _config.StopBits,
                Handshake = _config.Handshake,
                ReadTimeout = _config.ReadTimeout,
                WriteTimeout = _config.WriteTimeout,
                NewLine = "\r\n"
            };

            _serial = new SerialPortConnection(port);
            _serial.DataReceived += SerialPort_DataReceived;

            _log.Information("[GrblCoreService] SerialPort ready on {Port}", _config.PortName);
        }


        /// <summary>
        /// Tente d'établir une connexion avec la machine GRBL et effectue le handshake initial.
        /// </summary>
        /// <exception cref="GrblConnectionException">Levée si le port est occupé ou si la machine ne répond pas.</exception>
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;
            if (string.IsNullOrWhiteSpace(_config.PortName))
            {
                _log.Error("[GrblCoreService] Connection aborted: No COM port defined in configuration.");
                throw new GrblConnectionException(
                    GrblConnectionError.PortNotDefined,
                    "Aucun port COM sélectionné");
            }
            InitializeSerialPort();

            try
            {
                _log.Information("[GrblCoreService] Opening connection to GRBL on {Port} ({Baud} baud)...", _config.PortName, _config.BaudRate);
                await Task.Run(() => _serial!.Open());
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new GrblConnectionException(
                    GrblConnectionError.PortBusy,
                    $"Le port {_config.PortName} est déjà utilisé",
                    ex);
            }
            catch (IOException ex)
            {
                _log.Error("[GrblCoreService] Failed to open serial port {Port}", _config.PortName);
                throw new GrblConnectionException(
                    GrblConnectionError.PortNotAvailable,
                    $"Impossible d’ouvrir le port {_config.PortName}",
                    ex);
            }

            _grblHandshakeTcs = new TaskCompletionSource<bool>();

            await Task.Delay(500);
            try
            {
                _config.ClearBuffer();
                await SendRealtimeCommandAsync(0x18);
                await Task.Delay(1200);
                await SendCommandAsync("$I");
            }
            catch (TimeoutException ex)
            {
                throw new GrblConnectionException(
                    GrblConnectionError.NoResponse,
                    "Aucune réponse de la machine",
                    ex);
            }

            var completed = await Task.WhenAny(
                _grblHandshakeTcs.Task,
                Task.Delay(2000)
            );
                
                if (completed != _grblHandshakeTcs.Task || !_grblHandshakeTcs.Task.Result)
            {
                _log.Error("[GrblCoreService] Handshake Timeout: Machine did not respond with 'Grbl X.X' or [VER:]");
                throw new GrblConnectionException(
            GrblConnectionError.NotAGrblDevice,
            "Le périphérique connecté n’est pas une machine GRBL");
            }
            await GetSettingsAsync();
            HasLoadedSettings = true;
            _log.Information("[GrblCoreService] Connected on {port}",_config.PortName);
            await SendCommandAsync("M5 S0");
            OnPropertyChanged(nameof(IsConnected));
            ConnectionStateChanged?.Invoke(this, true);


        }
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;
            try
            {
                await SendCommandAsync("M5 S0");

                await Task.Run(() =>
                {
                    _serial!.DataReceived -= SerialPort_DataReceived;
                    _serial!.Close();
                });
                _serviceProvider.GetRequiredService<IGrblCommandQueue>().Reset();

                _log.Information("[GrblCoreService] Disconnected");
            }
            catch (Exception ex)
            {
                _log.Error("[GrblCoreService] Disconnection failed: {Message}", ex.Message);
                throw;
            }
            finally
            {
                HasLoadedSettings = false;
                OnPropertyChanged(nameof(IsConnected));
                ConnectionStateChanged?.Invoke(this, false);
            }
            
        }

        private void SerialPort_DataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serial?.IsOpen != true)
                    return;

                while (_serial.BytesToRead > 0)
                {
                    var line = _serial.ReadLine().Trim();
                    _rxRingBuffer.Push(line);

                    if (_log.IsSupportEnabled)
                    {
                        _log.Debug("[SUPPORT][RX-BUFFER] {Buffer}", string.Join(" | ", _rxRingBuffer));
                    }
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(line));
                    DispatchLine(line);
                    if (!line.StartsWith("<"))
                    { 
                    _log.Debug("[GrblCoreService RAW RX] {Line}", line);
                }
                }
                

            }
            catch (Exception ex)
            {
                _log.Error("[GrblCoreService] RX error: {Message}", ex.Message);
            }
        }
        private void DispatchLine(string line)
        {
            if (line.StartsWith("[VER:") || line.StartsWith("Grbl"))
            {
                _log.Debug("[GrblCoreService] Handshake signature detected: {Line}", line);
                _grblHandshakeTcs?.TrySetResult(true);
            }

            if (line.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(line.Split(':')[1], out int code))
                {
                    _state.MachineState = MachState.Alarm;
                    _state.MachineStatusColor = System.Windows.Media.Brushes.Red;
                    StatusUpdated?.Invoke(this, EventArgs.Empty);
                    _log.Warning("[GrblCoreService] Forced state to Alarm on RX after an alarm: {Line}", line);
                    var polling = _serviceProvider.GetRequiredService<IStatusPollingService>();
                    polling.ForcePoll();
                    Task.Delay(100).Wait();
                }
                return;
            }
            if (line.StartsWith("error:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(line.Split(':')[1], out int code))
                {
                    //_state.MachineState = MachState.Alarm;
                    //_state.AlarmCode = code; // Ajoute si besoin une prop dans GrblState
                    //StatusUpdated?.Invoke(this, EventArgs.Empty);
                    
                }
                _log.Warning("[GrblCoreService] Error non bloquante: {Line}", line);
                return;
            }
            _log.Debug("[GrblCoreService DispatchLine Entry] Processing line: {Line}", line);
            
            foreach (var parser in _parsers)
            {
                if (!parser.CanParse(line))
                    continue;

                parser.Parse(line, _state);

                if (parser is GrblStateParser)
                {
                    OnPropertyChanged(nameof(State));
                    StatusUpdated?.Invoke(this, EventArgs.Empty);
                    StatusLineReceived?.Invoke(this, EventArgs.Empty);

                }
                else if (parser is GrblSettingsParser)
                {
                    SettingsUpdated?.Invoke(this, _state.Settings.Values.ToList());
                }

                return;
            }
        }


        // ----------------------------------------------------
        // TX : COMMAND SENT
        // ----------------------------------------------------
        public async Task SendCommandAsync(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("[GrblCoreService] Not connected");

            if (_log.IsCncEnabled)
            {
                _log.Debug("[CNC][CORE][TX] {Command}", command);
            }
            DataReceived?.Invoke(this, new DataReceivedEventArgs($">> {command}"));

            await Task.Run(() => _serial?.WriteLine(command));
        }

        public async Task SendRealtimeCommandAsync(byte command)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[GrblCoreService] Not connected.");

            if (command!= 0x3F)
            {
                _log.Information("[GrblCoreService TX RT] 0x{Cmd:X2}", command);
            }
            

            await Task.Run(() => _serial?.Write(new[] { command }, 0, 1));
        }
        public void SendLine(string command)
        {
            if (!_serial.IsOpen)
                return;

            _log.Information("[GrblCoreService TX] {Cmd}", command);

            _serial!.WriteLine(command);
        }


        // ----------------------------------------------------
        // COMMON COMMANDS
        // ----------------------------------------------------
        public Task HomeAsync() => SendCommandAsync("$H");
        public Task UnlockAsync() => SendCommandAsync("$X");
        public Task GetSettingsAsync() => SendCommandAsync("$$");
        internal GrblRxRingBuffer RxRingBuffer => _rxRingBuffer;
        public bool IsLastErrorAfterOk()
        {
            return _rxRingBuffer.ErrorAfterOk();
        }

        public int? GetLastRxErrorCode()
        {
            return _rxRingBuffer.LastErrorCode();
        }


        // ----------------------------------------------------
        // NOTIFY PROPERTY CHANGED
        // ----------------------------------------------------
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void MarkSettingsLoaded()
        {
            HasLoadedSettings = true;
        }
        public void RaiseDataReceived(string text)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(text));
        }
        // ----------------------------------------------------
        // DISPOSE
        // ----------------------------------------------------
        public void Dispose()
        {
            try
            {
                DisconnectAsync().GetAwaiter().GetResult();
                _serial?.Dispose();
            }
            catch { }
        }
#if DEBUG
        // ----------------------------------------------------
        // FOR UNIT TEST GETSETTINGS
        // ----------------------------------------------------
        
        internal void SetSerialConnectionForTests(ISerialConnection serial)
        {
            _serial = serial;
            _serial.DataReceived += SerialPort_DataReceived;
        }
#if DEBUG
        public void ProcessIncomingLine_ForTests(string line)
        {
            // Appelez directement la vraie méthode de traitement
            DispatchLine(line);
        }
#endif
#endif


    }
}

