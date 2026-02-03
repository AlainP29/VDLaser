using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO.Ports;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// ViewModel de configuration du port série.
    /// Implémente ISerialPortConfig pour être directement injecté dans GrblCoreService.
    /// </summary>
    public partial class SerialPortSettingViewModel : ViewModelBase
    {
        #region Private Fields & Observables
        private readonly ILogService _log;

        private readonly ISerialPortService _serialService;

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
        private int _readTimeout = 500;

        [ObservableProperty]
        private int _writeTimeout = 2000;

        [ObservableProperty]
        private bool _isRefreshingPort;
        
        public event EventHandler? SettingsChanged;
        #endregion
        public SerialPortSettingViewModel(ISerialPortService serialService, ILogService log)
        {
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));
            InitializeFromService();
            _log = log;
            _log.Information("[SerialPortSettingViewModel] Initialised");
            _serialService.SettingsChanged += OnServiceSettingsChanged;
        }

        // Init ViewModel from service state
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
        }

        /// <summary>
        /// When SerialPortService refreshes ports or updates config
        /// </summary>
        private void OnServiceSettingsChanged(object? sender, EventArgs e)
        {
            InitializeFromService();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called by Refresh button in UI
        /// </summary>
        [RelayCommand]
        public async Task RefreshPortsAsync()
        {
            IsRefreshingPort = true;
            try
            {
                await Task.Run(() => _serialService.RefreshPortNames());
                _log.Information("[SerialPortSettingViewModel] refresh ports");
            }
            finally
            {
                IsRefreshingPort = false;
            }
        }

        /// <summary>
        /// Applies UI-changed settings back into SerialPortService
        /// </summary>
        public void ApplySettings()
        {
            _serialService.PortName = PortName;
            _serialService.BaudRate = BaudRate;
            _serialService.Parity = Parity;
            _serialService.DataBits = DataBits;
            _serialService.StopBits = StopBits;
            _serialService.Handshake = Handshake;
            _serialService.ReadTimeout = ReadTimeout;
            _serialService.WriteTimeout = WriteTimeout;

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
