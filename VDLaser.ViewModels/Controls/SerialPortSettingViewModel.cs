using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO.Ports;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    // <summary>
    /// Manages the configuration of the serial communication port.
    /// Implements settings persistence for the ISerialPortService.
    /// </summary>
    public partial class SerialPortSettingViewModel : ViewModelBase
    {
        #region Fields & Services
        private readonly ILogService _log;
        private readonly ISerialPortService _serialService;

        public event EventHandler? SettingsChanged;
        public event EventHandler? ProfileChanged;
        #endregion

        #region Properties
        [ObservableProperty]
        private List<string> _listPortNames = new();

        [ObservableProperty]
        private string _portName = string.Empty;

        [ObservableProperty]
        private List<int> _listBaudRates = new()
        {
            1200, 2400, 4800, 9600,
            19200, 38400, 57600, 115200, 230400
        };

        [ObservableProperty]
        private int _baudRate = 115200;

        [ObservableProperty]
        private Parity _parity = Parity.None;

        [ObservableProperty]
        private int _dataBits = 8;

        [ObservableProperty]
        private StopBits _stopBits = StopBits.One;

        [ObservableProperty]
        private Handshake _handshake = Handshake.None;

        [ObservableProperty]
        private int _readTimeout = 1000;

        [ObservableProperty]
        private int _writeTimeout = 2000;

        [ObservableProperty]
        private bool _isRefreshingPort;
        #endregion

        public SerialPortSettingViewModel(ISerialPortService serialService, ILogService log)
        {
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));
            _log = log;

            InitializeFromService();
            _serialService.SettingsChanged += OnServiceSettingsChanged;
            _serialService.ProfileChanged += HandleProfileChanged;

            LogContextual(_log, "Initialized", "Serial port settings loaded");
        }

        #region logic
        private void InitializeFromService()
        {
            ListPortNames = new List<string>(_serialService.ListPortNames);
            ListBaudRates = new List<int>(_serialService.ListBaudRates);

            PortName = _serialService.PortName;
            BaudRate = _serialService.BaudRate;
            Parity = _serialService.Parity;
            DataBits = _serialService.DataBits;
            StopBits = _serialService.StopBits;
            Handshake = _serialService.Handshake;

            ReadTimeout = _serialService.ReadTimeout;
            WriteTimeout = _serialService.WriteTimeout;

            LogContextual(_log, "InitializeFromService", $"UI synchronized with Service. Port: {PortName}, Profile: {_serialService.CurrentProfile}");
        }

        private void OnServiceSettingsChanged(object? sender, EventArgs e)
        {
            InitializeFromService();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        private void HandleProfileChanged(object? sender, LogProfile profile)
        {
            ReadTimeout = _serialService.ReadTimeout;
            WriteTimeout = _serialService.WriteTimeout;

            LogContextual(_log, "TimeoutSynced", $"UI synced to {profile}: Read={ReadTimeout}ms");
        }
        public void ApplySettings()
        {
            LogContextual(_log, "ApplySettings", $"Port: {PortName}, Baud: {BaudRate}");

            _serialService.PortName = PortName;
            _serialService.BaudRate = BaudRate;
            _serialService.Parity = Parity;
            _serialService.DataBits = DataBits;
            _serialService.StopBits = StopBits;
            _serialService.Handshake = Handshake;
            _serialService.ReadTimeout = ReadTimeout;
            _serialService.WriteTimeout = WriteTimeout;

            SettingsChanged?.Invoke(this, EventArgs.Empty);

            LogContextual(_log, "ApplySettings", "User applied manual serial settings from UI");
        }

        #endregion

        #region commands
        [RelayCommand]
        public async Task RefreshPortsAsync()
        {
            LogContextual(_log, "RefreshPorts", "Scanning available serial ports");
            IsRefreshingPort = true;
            try
            {
                await Task.Run(() => _serialService.RefreshPortNames());
                ListPortNames = _serialService.GetAvailablePorts().ToList();
            }
            finally
            {
                IsRefreshingPort = false;
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serialService.SettingsChanged -= OnServiceSettingsChanged;
                _serialService.ProfileChanged -= HandleProfileChanged;
            }
            base.Dispose(disposing);
        }
    }
}
