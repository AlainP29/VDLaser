using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using VDGrbl.Model;
using System.IO.Ports;
using System.Collections.Generic;
using System;
using System.Windows;
using NLog;

namespace VDGrbl.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        #region private Members
        private readonly IDataService _dataService;
        private SerialPort _serialPort;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //private readonly SerialPortSettingsModel DeviceComPort = new SerialPortSettingsModel();
        private string _selectedDevicePortName=string.Empty;
        private string[] _listPortNames;
        private int _selectedBaudRate;
        private int[] _listBaudRates;
        private int _selectedDataBits;
        private int[] _listDataBits;
        private string _groupBoxPortSettingsTitle = string.Empty;
        private Parity _selectedParity;
        private List<SerialPortSettingsModel> _listParities;
        private StopBits _selectedStopBits;
        private List<SerialPortSettingsModel> _listStopBits;
        private Handshake _selectedHandshake;
        private List<SerialPortSettingsModel> _listHandshake;
        private string _txLine = string.Empty;
        private string _rxLine = string.Empty;
        private readonly char[] trimArray = new char[] { '\r', '\n', ' ' };
        #endregion

        #region Commands
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        public RelayCommand SendCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }
        #endregion

        #region public Properties
        /// <summary>
        /// The <see cref="GroupBoxPortSettingsTitle" /> property's name.
        /// </summary>
        //public const string GroupBoxPortSettingsTitlePropertyName = "GroupBoxPortSettingsTitle";

        /// <summary>
        /// Gets the GroupBoxPortSettingsTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxPortSettingsTitle
        {
            get
            {
                return _groupBoxPortSettingsTitle;
            }
            set
            {
                Set(ref _groupBoxPortSettingsTitle, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedBaudRate" /> property's name.
        /// </summary>
        public const string SelectedBaudRatePropertyName = "SelectedBaudRate";

        /// <summary>
        /// Gets the SelectedBaudRateName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SelectedBaudRate
        {
            get
            {
                return _selectedBaudRate;
            }
            set
            {
                Set(ref _selectedBaudRate, value);
            }
        }

        /// <summary>
        /// The <see cref="_listBaudRates" /> property's name.
        /// </summary>
        public const string ListBaudRatesPropertyName = "ListBaudRates";

        /// <summary>
        /// Gets the ListBaudRates property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int[] ListBaudRates
        {
            get
            {
                return _listBaudRates;
            }
            set
            {
                Set(ref _listBaudRates, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedDataBits" /> property's name.
        /// </summary>
        public const string SelectedDataBitsPropertyName = "SelectedDataBits";

        /// <summary>
        /// Gets the SelectedBaudRateName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SelectedDataBits
        {
            get
            {
                return _selectedDataBits;
            }
            set
            {
                Set(ref _selectedDataBits, value);
            }
        }

        /// <summary>
        /// The <see cref="_listDataBits" /> property's name.
        /// </summary>
        public const string ListDataBitsPropertyName = "ListDataBits";

        /// <summary>
        /// Gets the ListDataBits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int[] ListDataBits
        {
            get
            {
                return _listDataBits;
            }
            set
            {
                Set(ref _listDataBits, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedParity" /> property's name.
        /// </summary>
        public const string SelectedParityPropertyName = "SelectedParity";

        /// <summary>
        /// Gets the SelectedParityName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Parity SelectedParity
        {
            get
            {
                return _selectedParity;
            }
            set
            {
                Set(ref _selectedParity, value);
            }
        }

        /// <summary>
        /// The <see cref="_listParities" /> property's name.
        /// </summary>
        public const string ListParitiesPropertyName = "ListParities";

        /// <summary>
        /// Gets the ListParities property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<SerialPortSettingsModel> ListParities
        {
            get
            {
                return _listParities;
            }
            set
            {
                Set(ref _listParities, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedStopBits" /> property's name.
        /// </summary>
        public const string SelectedPropertyName = "SelectedStopBits";

        /// <summary>
        /// Gets the SelectedBaudRateName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public StopBits SelectedStopBits
        {
            get
            {
                return _selectedStopBits;
            }
            set
            {
                Set(ref _selectedStopBits, value);
            }
        }

        /// <summary>
        /// The <see cref="_listStopBits" /> property's name.
        /// </summary>
        public const string ListStopBitsPropertyName = "ListStopBits";

        /// <summary>
        /// Gets the ListParities property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<SerialPortSettingsModel> ListStopBits
        {
            get
            {
                return _listStopBits;
            }
            set
            {
                Set(ref _listStopBits, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedHandshake" /> property's name.
        /// </summary>
        public const string SelectedHandshakePropertyName = "SelectedHandshake";

        /// <summary>
        /// Gets the SelectedBaudRateName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Handshake SelectedHandshake
        {
            get
            {
                return _selectedHandshake;
            }
            set
            {
                Set(ref _selectedHandshake, value);
            }
        }

        /// <summary>
        /// The <see cref="_listHandshake" /> property's name.
        /// </summary>
        public const string ListHandshakePropertyName = "ListHandshake";

        /// <summary>
        /// Gets the ListParities property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<SerialPortSettingsModel> ListHandshake
        {
            get
            {
                return _listHandshake;
            }
            set
            {
                Set(ref _listHandshake, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedPortName" /> property's name.
        /// </summary>
        public const string SelectedPortNamePropertyName = "SelectedPortName";

        /// <summary>
        /// Gets the SelectedPortName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedPortName
        {
            get
            {
                return _selectedDevicePortName;
            }
            set
            {
                Set(ref _selectedDevicePortName, value);
            }
        }

        /// <summary>
        /// The <see cref="_listPortNames" /> property's name.
        /// </summary>
        public const string ListPortNamesPropertyName = "ListPortNames";

        /// <summary>
        /// Gets the ListParities property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string[] ListPortNames
        {
            get
            {
                return _listPortNames;
            }
            set
            {
                Set(ref _listPortNames, value);
            }
        }

        /// <summary>
        /// The <see cref="_txLine" /> property's name.
        /// </summary>
        public const string TXLinePropertyName = "TXLine";

        /// <summary>
        /// Gets the TXLine property. TXLine is the transmetted G-Code or Grbl line of data
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TXLine
        {
            get
            {
                return _txLine;
            }
            set
            {
                Set(ref _txLine, value);
            }
        }

        /// <summary>
        /// The <see cref="_rxLine" /> property's name.
        /// </summary>
        public const string RXLinePropertyName = "RXLine";

        /// <summary>
        /// Gets the RXLine property. RXLine is the data received from the controller mostly a Grbl answer
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RXLine
        {
            get
            {
                return _rxLine;
            }
            set
            {
                Set(ref _rxLine, value);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            try
            {
                logger.Log(LogLevel.Info, "---Program started ! ---");

                _dataService = dataService;
                _dataService.GetData(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                        // Report error here
                        return;
                        }
                        _serialPort = new SerialPort();
                        GroupBoxPortSettingsTitle = item.PortSettingsHeader;
                        GetSerialPortSettings(item);
                        DefaultPortSettings();
                        MyCommands();
                        logger.Info("Window initialized");

                    });
            }
            catch(Exception ex)
            {
                logger.Error("Exception constructor raised: " + ex.ToString());
            }
}
        #endregion

        #region Methods
        /// <summary>
        /// Get serial port settings from SerialPortSettingsModel class
        /// </summary>
        /// <param name="settingsInit"></param>
        private void GetSerialPortSettings(SerialPortSettingsModel settingsInit)
        {
            try
            {
                ListPortNames = settingsInit.ListPortNames;
                ListBaudRates = settingsInit.ListBaudRates;
                ListDataBits = settingsInit.ListDataBits;
                ListParities = settingsInit.ListParities();
                ListStopBits = settingsInit.ListStopBits();
                ListHandshake = settingsInit.ListHandshake();
                logger.Info("All settings loaded");

            }
            catch (Exception ex)
            {
                logger.Error("Exception get settings raised: " + ex.ToString());
            }
}

        /// <summary>
        /// List of RelayCommands 
        /// </summary>
        private void MyCommands()
        {
            ConnectCommand = new RelayCommand(Open, CanExecuteOpen);
            DisconnectCommand = new RelayCommand(Close, CanExecuteClose);
            SendCommand = new RelayCommand(SendData, CanExecuteSendData);
            ClearCommand = new RelayCommand(ClearData, CanExecuteClearData);
        }

        /// <summary>
        /// Set default settings for serial port
        /// </summary>
        private void DefaultPortSettings()
        {
            try
            {
                if (ListPortNames != null&&ListPortNames.Length>0)
                {
                    _selectedDevicePortName = ListPortNames[0];
                    logger.Log(LogLevel.Info, "No port COM available");

                }
                _selectedBaudRate = 9600;
                _selectedDataBits = 8;
                _selectedParity = Parity.None;
                _selectedStopBits = StopBits.One;
                _selectedHandshake = Handshake.None;
                logger.Log(LogLevel.Info, "Default settings loaded");

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "Exception default settings raised: " + ex.ToString());
            }
        }
        
        /// <summary>
        /// Start serial port communication
        /// </summary>
        public void Open()
        {
            try
            {
                if (String.IsNullOrEmpty(SelectedPortName))
                {
                    MessageBox.Show("Select a Port COM !");
                    logger.Info("Select a Port COM");
                }
                else
                {
                    _serialPort.PortName = SelectedPortName;
                    _serialPort.BaudRate = SelectedBaudRate;
                    _serialPort.Parity = SelectedParity;
                    _serialPort.StopBits = SelectedStopBits;
                    _serialPort.DataBits = SelectedDataBits;
                    _serialPort.Open();
                    logger.Info("Port COM open");
                    _serialPort.DataReceived += _serialPort_DataReceived;
                }
            }
            catch(Exception ex)
            {
                logger.Error("Exception open raised: " + ex.ToString());
            }
        }
        /// <summary>
        /// Allow/Disallow Open method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteOpen()
        {
            return !_serialPort.IsOpen;
        }

        /// <summary>
        /// End serial port communication
        /// </summary>
        public void Close()
        {
            try
            {
                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.Dispose();
                _serialPort.Close();
                logger.Info("Port COM closed");

            }
            catch (Exception ex)
            {
                logger.Error("Exception close raised: " + ex.ToString());
            }
        }
        /// <summary>
        /// Allow/Disallow Close method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteClose()
        {
            return _serialPort!=null&&_serialPort.IsOpen;
        }

        /// <summary>
        /// Send G-code or Grbl data to com port
        /// Rq: on peut également utiliser SendData() sans paramètre pour pouvoir utiliser RelayCommand sans paramètre et donc avec can execute _SerialPort.Write(InputText);InputText = String.Empty; OnPropertyChanged("InputText");
        /// </summary>
        public void SendData()
        {
            try
            {
                //_serialPort.WriteLine(TXLine);
                _serialPort.Write(TXLine);
                logger.Info("Data {0} sent",TXLine);

            }
            catch (Exception ex)
            {
                logger.Error("Exception send data raised: " + ex.ToString());
            }
}
        /// <summary>
        /// Allow/Disallow the Senddata method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteSendData()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(TXLine))
            {
                return true;
            }
                return false;
        }

        /// <summary>
        /// Clear TextBlock
        /// </summary>
        public void ClearData()
        {
            try
            { 
                RXLine = string.Empty;
                logger.Info("RXLine erased");

            }
            catch (Exception ex)
            {
                logger.Error("Exception clear data raised: " + ex.ToString());
            }
}
        /// <summary>
        /// Allow/Disallow the cleardata method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteClearData()
        {
            return true;
        }

        /// <summary>
        /// Clear all data in buffer in/out
        /// </summary>
        public void ClearQueue()
        {
            try
            { 
            if(_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                logger.Info("Buffer cleared");

                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception buffer raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Does nothing yet
        /// </summary>
        public override void Cleanup()
        {
            // Clean up if needed

            base.Cleanup();
        }

        public void Dispose()
        {
            try
            { 
                _serialPort.Dispose();
                logger.Info("Port COM disposed");
            }
            catch (Exception ex)
            {
                logger.Error("Exception dispose raised: " + ex.ToString());
            }
        }
        #endregion

        #region Event
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = _serialPort.ReadLine().ToLower().Trim().TrimEnd(trimArray);
                RXLine += line + Environment.NewLine;
                logger.Info("Data {0} received2", line);
                if(line.Length > 0)
                {
                    if(line.StartsWith("ok"))
                    {
                        //TODO : send next line
                    }
                    else if (line.StartsWith("error"))
                    {
                        //TODO : send next line
                    }
                    else if (line.StartsWith("<") && RXLine.EndsWith(">"))
                    {
                        //TODO : check version and extract position and status
                    }
                    else if (line.StartsWith("$"))
                    {
                        //TODO : get Grbl settings
                    }
                    else if (line.StartsWith("["))
                    {
                        //TODO : get version, build infos and help message
                    }
                    else { }
                }

            }
            catch(Exception ex)
                {
                logger.Error("Exception data received raised: " + ex.ToString());
            }
        }
        #endregion  
        
    }
}