using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.IO.Ports;
using VDLaser.Core.Console;
using VDLaser.Core.Grbl.Errors;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Core.Grbl.Services
{
    /// <summary>
    /// Core service for managing GRBL device communication over a serial port.
    /// </summary>
    public partial class GrblCoreService : ObservableObject, IGrblCoreService, IDisposable 
    {
        #region Fields & Properties
        private readonly ISerialPortService _config;
        private readonly ILogService _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IGrblSubParser> _parsers;
        private readonly IConsoleParserService _consoleParser;

        private ISerialConnection? _serial;
        private readonly GrblState _state = new();
        private readonly GrblInfo _grblInfo = new();

        
        public event EventHandler<DataReceivedEventArgs>? DataReceived;
        public event EventHandler? StatusUpdated;
        public event EventHandler<IReadOnlyCollection<GrblSetting>>? SettingsUpdated;
        public event EventHandler<GrblInfo>? InfoUpdated;
        
        public event EventHandler<bool>? ConnectionStateChanged;
        public event EventHandler? StatusLineReceived;

        public bool IsConnected => _serial?.IsOpen == true;
        public GrblState State => _state;
        public GrblInfo GrblInfo => _grblInfo;

        public bool IsLaserPower { get; private set; } = false;
        [ObservableProperty]
        private bool _hasLoadedSettings = false;
        
        private TaskCompletionSource<bool>? _grblHandshakeTcs;
        private readonly GrblRxRingBuffer _rxRingBuffer = new();
        private readonly SemaphoreSlim _serialSemaphore = new SemaphoreSlim(1, 1);
        #endregion

        #region Constructor
        public GrblCoreService
            (
            ISerialPortService config,
            ILogService log,
            IEnumerable<IGrblSubParser> parsers,
            IServiceProvider serviceProvider,
            IConsoleParserService consoleParser,
            ISerialConnection? serialOverride = null
            )
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _parsers = parsers?.ToList() ?? throw new ArgumentNullException(nameof(parsers));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _consoleParser= consoleParser ?? throw new ArgumentNullException(nameof(consoleParser));
            _serial = serialOverride;//injection mock pour tests unitaires

            _log.Information("[CORE] Initialized");
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Sets up the serial port connection using the configuration settings.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
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

            _log.Information("[CORE] SerialPort ready on {Port}", _config.PortName);
        }

        /// <summary>
        /// Asynchronously connects to the GRBL device using the configured serial port settings.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GrblConnectionException"></exception>
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;
            if (string.IsNullOrWhiteSpace(_config.PortName))
            {
                _log.Error("[CORE] Connection aborted: No COM port defined in configuration.");
                throw new GrblConnectionException(
                    GrblConnectionError.PortNotDefined,
                    "No COM port selected");
            }
            if (_serial == null)
                InitializeSerialPort();

            try
            {
                _log.Information("[CORE] Opening connection to GRBL on {Port} ({Baud} baud)...", _config.PortName, _config.BaudRate);
                await Task.Run(() => _serial!.Open());
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new GrblConnectionException(
                    GrblConnectionError.PortBusy,
                    $"The port {_config.PortName} is already in use",
                    ex);
            }
            catch (IOException ex)
            {
                _log.Error("[CORE] Failed to open serial port {Port}", _config.PortName);
                throw new GrblConnectionException(
                    GrblConnectionError.PortNotAvailable,
                    $"The port cannot be opened {_config.PortName}",
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
                    "No response from the machine",
                    ex);
            }

            var completed = await Task.WhenAny(
                _grblHandshakeTcs.Task,
                Task.Delay(2000)
            );
                
                if (completed != _grblHandshakeTcs.Task || !_grblHandshakeTcs.Task.Result)
            {
                _log.Error("[CORE] Handshake Timeout: Machine did not respond with 'Grbl X.X' or [VER:]");
                throw new GrblConnectionException(
            GrblConnectionError.NotAGrblDevice,
            "The connected device is not a GRBL machine");
            }
            await GetSettingsAsync();
            HasLoadedSettings = true;
            _log.Information("[CORE] Connected on {port}", _config.PortName);
            await SendCommandAsync("M5 S0");
            OnPropertyChanged(nameof(IsConnected));
            ConnectionStateChanged?.Invoke(this, true);
        }

        /// <summary>
        /// Asynchronously disconnects from the GRBL device and releases associated resources.
        /// </summary>
        /// <remarks>If the service is not currently connected, the method returns immediately without
        /// performing any action.  After disconnection, the command queue is reset and connection state change events
        /// are raised. </remarks>
        /// <returns>A task that represents the asynchronous disconnect operation.</returns>
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
                _serviceProvider.GetRequiredService<IGrblCommandQueue>().Reset();//No DI to avoid circular reference, get directly from provider

                _log.Information("[CORE] Disconnected");
            }
            catch (Exception ex)
            {
                _log.Error("[CORE] Disconnection failed: {Message}", ex.Message);
                throw;
            }
            finally
            {
                HasLoadedSettings = false;
                OnPropertyChanged(nameof(IsConnected));
                ConnectionStateChanged?.Invoke(this, false);
            }
            
        }

        /// <summary>
        /// Processes a single line of input from the GRBL device, updating internal state and raising relevant events
        /// based on the content of the line.
        /// </summary>
        /// <remarks>This method interprets handshake, alarm, error, and status lines from the GRBL
        /// device. Depending on the content, it updates the machine state, triggers status or settings updates, and
        /// logs relevant information. If the line matches a known parser, the corresponding state or settings events
        /// are raised. This method is intended for internal use within the service to handle device
        /// communication.</remarks>
        /// <param name="line">The line of text received from the GRBL device to be parsed and dispatched. Cannot be <see
        /// langword="null"/>.</param>
        private void DispatchLine(string line)
        {
            if (line.StartsWith("[VER:") || line.StartsWith("Grbl"))
            {
                _log.Debug("[CORE] Handshake signature detected: {Line}", line);
                DataReceived?.Invoke(this, new DataReceivedEventArgs(line));
                _grblHandshakeTcs?.TrySetResult(true);
            }

            if (line.StartsWith("ALARM:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(line.Split(':')[1], out int code))
                {
                    _state.MachineState = MachState.Alarm;
                    _state.MachineStatusColor = System.Windows.Media.Brushes.Red;
                    StatusUpdated?.Invoke(this, EventArgs.Empty);

                    _log.Warning("[CORE] Forced state to Alarm on RX after an alarm: {Line}", line);

                    var polling = _serviceProvider.GetRequiredService<IStatusPollingService>();//No DI to avoid circular reference, get directly from provider
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
                _log.Warning("[CORE] Error non bloquante: {Line}", line);
                return;
            }
            if (!line.StartsWith("<") && _log.IsCncEnabled)
            {
                _log.Debug("[CNC][CORE][RAW RX] {Line}", line);
            }
            _log.Debug("[CORE] DispatchLine Entry - Processing line: {Line}", line);
            
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

        /// <summary>
        /// Gcode TX command to GRBL
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendCommandAsync(string command)
        {
            if (!IsConnected) throw new InvalidOperationException("[CORE] Not connected");

            await _serialSemaphore.WaitAsync();
            try
            {
                if (_log.IsCncEnabled)
                {
                    _log.Debug("[CNC][CORE][TX] {Command}", command);
                }

                _consoleParser.BeginCommand(command);
                if (_consoleParser.CurrentPendingCommand != null)
                    _consoleParser.CurrentPendingCommand.Source = ConsoleSource.Manual;

                await Task.Run(() => _serial?.WriteLine(command));
            }
            finally
            {
                _serialSemaphore.Release();
            }
         }

        /// <summary>
        /// Realtime TX command to GRBL
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendRealtimeCommandAsync(byte command)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[CORE] Not connected.");

            await _serialSemaphore.WaitAsync();
            try { 
            if (command != 0x3F)
            {
                _log.Information("[CORE][TX RT] 0x{Cmd:X2}", command);
            }

            await Task.Run(() => _serial?.Write(new[] { command }, 0, 1));
        }
            finally
            {
                _serialSemaphore.Release();
            }
        }

        /// <summary>
        /// Sends the specified command to the serial port, appending a line terminator.
        /// </summary>
        /// <remarks>The command is transmitted only if the serial port is open. A line terminator is
        /// automatically appended to the command before transmission.</remarks>
        /// <param name="command">The command string to send. Cannot be <see langword="null"/>.</param>
        public void SendLine(string command)
        {
            if (!_serial.IsOpen)
                return;

            _log.Information("[CORE][TX] {Cmd}", command);

            _serial!.WriteLine(command);
        }

        public Task HomeAsync() => SendCommandAsync("$H");
        public Task UnlockAsync() => SendCommandAsync("$X");
        public Task GetSettingsAsync() => SendCommandAsync("$$");
        internal GrblRxRingBuffer RxRingBuffer => _rxRingBuffer;

        /// <summary>
        /// Determines whether the most recent error in the receive buffer occurred after a successful operation.
        /// </summary>
        /// <remarks>Use this method to check the sequence of events in the receive buffer, particularly
        /// to verify if an error has occurred following a successful operation. This can be useful for error handling
        /// or diagnostics.</remarks>
        /// <returns><see langword="true"/> if the last error was recorded after a successful operation; otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsLastErrorAfterOk()
        {
            return _rxRingBuffer.ErrorAfterOk();
        }

        /// <summary>
        /// Gets the most recent error code encountered during receive operations, if any.
        /// </summary>
        /// <returns>The last receive error code as an integer, or <see langword="null"/> if no error has occurred.</returns>
        public int? GetLastRxErrorCode()
        {
            return _rxRingBuffer.LastErrorCode();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the <see cref="System.IO.Ports.SerialPort.DataReceived"/> event and processes incoming serial data.
        /// </summary>
        /// <remarks>This method reads available lines from the serial port, adds them to an internal
        /// receive buffer, logs received data if support logging is enabled, and raises the <c>DataReceived</c> event
        /// for each line. It also dispatches each received line for further processing.</remarks>
        /// <param name="sender">The source of the event, typically the serial port instance.</param>
        /// <param name="e">An object that contains the event data.</param>
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
                        _log.Debug("[SUPPORT][CORE][RX-BUFFER] {Buffer}", string.Join(" | ", _rxRingBuffer));
                        _log.Debug("[SUPPORT][CORE][RAW RX] {Line}", line);
                    }
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(line));
                    DispatchLine(line);
                    
                }


            }
            catch (Exception ex)
            {
                _log.Error("[CORE] RX error: {Message}", ex.Message);
            }
        }
        /// <summary>
        /// Marks the settings as loaded.
        /// </summary>
        /// <remarks>Sets the internal state to indicate that the settings have been successfully loaded. 
        /// Call this method after completing the loading process to update the status.</remarks>
        public void MarkSettingsLoaded()
        {
            HasLoadedSettings = true;
        }
        /// <summary>
        /// Raises the <see cref="DataReceived"/> event with the specified text data.
        /// </summary>
        /// <param name="text">The text data to include in the <see cref="DataReceivedEventArgs"/> when raising the event. Can be <see
        /// langword="null"/>.</param>
        public void RaiseDataReceived(string text)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(text));
        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            try
            {
                DisconnectAsync().GetAwaiter().GetResult();
                _serial?.Dispose();
            }
            catch { }
        }
        #endregion

        #region Debug Methods
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

        #endregion
    }
}

