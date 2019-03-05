using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VDGrbl.Codes;
using VDGrbl.Model;
using VDGrbl.Tools;
using System.Windows.Data;
using System.Globalization;

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
        #region private Fields
        private readonly IDataService _dataService;
        private SerialPort _serialPort;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //private readonly SerialPortSettingsModel DeviceComPort = new SerialPortSettingsModel();
        private string _selectedDevicePortName = string.Empty;
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
        private RespStatus _responseStatus = RespStatus.Ok;//TODO softwareStatus, responseStatus tobe check depending on what we want to include in checking...
        private MachStatus _machineStatus = MachStatus.Idle;
        private SolidColorBrush _machineStatusColor = new SolidColorBrush(Colors.LightGray);
        private string _versionGrbl = "-";
        private string _buildInfo = "-";
        private string _posX = "0.000";
        private string _posY = "0.000";
        private string _posZ = "0.000";
        private ObservableCollection<SettingModel> _settingCollection;
        private List<SettingModel> _listSettingModel = new List<SettingModel>();
        private SettingModel _settingModel;
        private double _feedRate = 300;
        private string _step = "1";
        private bool _isSelectedKeyboard=false;
        private bool _isJogEnabled=true;
        #endregion

        #region public Properties
        #region subregion enum
        public enum RespStatus { Q, DR, Ok, NOk };//Q: Queued, DR: Data received
        public enum MachStatus { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep };
        #endregion

        #region subregion Relaycommands
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        public RelayCommand SendCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand GrblResetCommand { get; private set; }
        public RelayCommand GrblPauseCommand { get; private set; }
        public RelayCommand GrblCurrentStatusCommand { get; private set; }
        public RelayCommand GrblStartCommand { get; private set; }
        public RelayCommand GrblSettingsCommand { get; private set; }
        public RelayCommand GrblParametersCommand { get; private set; }
        public RelayCommand GrblParserStateCommand { get; private set; }
        public RelayCommand GrblBuildInfoCommand { get; private set; }
        public RelayCommand GrblStartupBlocksCommand { get; private set; }
        public RelayCommand GrblCheckCommand { get; private set; }
        public RelayCommand GrblKillAlarmCommand { get; private set; }
        public RelayCommand GrblHomingCommand { get; private set; }
        public RelayCommand GrblSleepCommand { get; private set; }
        public RelayCommand GrblTestCommand { get; private set; }
        public RelayCommand GrblHelpCommand { get; private set; }
        public RelayCommand<bool> JogHCommand { get; private set; }
        public RelayCommand<bool> JogNCommand { get; private set; }
        public RelayCommand<bool> JogSCommand { get; private set; }
        public RelayCommand<bool> JogECommand { get; private set; }
        public RelayCommand<bool> JogWCommand { get; private set; }
        public RelayCommand<bool> JogNWCommand { get; private set; }
        public RelayCommand<bool> JogNECommand { get; private set; }
        public RelayCommand<bool> JogSWCommand { get; private set; }
        public RelayCommand<bool> JogSECommand { get; private set; }
        public RelayCommand<bool> JogUpCommand { get; private set; }
        public RelayCommand<bool> JogDownCommand { get; private set; }
        public RelayCommand<string> StepCommand { get; private set; }
        public RelayCommand<bool> IncreaseFeedRateCommand { get; private set; }
        public RelayCommand<bool> DecreaseFeedRateCommand { get; private set; }
        public RelayCommand ResetAxisXCommand { get; private set; }
        public RelayCommand ResetAxisYCommand { get; private set; }
        public RelayCommand ResetAxisZCommand { get; private set; }
        public RelayCommand ResetAllAxisCommand { get; private set; }

        #endregion

        #region subregion port settings
        /// <summary>
        /// The <see cref="GroupBoxPortSettingsTitle" /> property's name.
        /// </summary>
        public const string GroupBoxPortSettingsTitlePropertyName = "GroupBoxPortSettingsTitle";

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
        /// The <see cref="ListBaudRates" /> property's name.
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
        /// The <see cref="ListDataBits" /> property's name.
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
        /// The <see cref="ListParities" /> property's name.
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
        /// The <see cref="ListStopBits" /> property's name.
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
        /// The <see cref="ListHandshake" /> property's name.
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
        /// The <see cref="ListPortNames" /> property's name.
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
        #endregion

        #region subregion Data TX/RX/Settings
        /// <summary>
        /// The <see cref="TXLine" /> property's name.
        /// </summary>
        public const string TXLinePropertyName = "TXLine";
        /// <summary>
        /// Gets the TXLine property. TXLine is the transmetted G-Code or Grbl commands to the Arduino
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
        /// The <see cref="RXLine" /> property's name.
        /// </summary>
        public const string RXLinePropertyName = "RXLine";
        /// <summary>
        /// Gets the RXLine property. RXLine is the data received from the Arduino
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

        /// <summary>
        /// The <see cref="ListSettingModel" /> property's name.
        /// </summary>
        public const string ListSettingModelName = "ListSettingModel";
        /// <summary>
        /// Gets the ListSettingModel property. ListSettingsModel is populated w/ Grbl settings data ('$$' command)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<SettingModel> ListSettingModel
        {
            get
            {
                return _listSettingModel;
            }
            set
            {
                Set(ref _listSettingModel, value);
            }
        }

        /// <summary>
        /// The <see cref="SettingCollection" /> property's name.
        /// </summary>
        public const string SettingCollectionName = "SettingCollection";
        /// <summary>
        /// Gets the SettingCollection property. SettingCollection is populated w/ data from ListSettingModel
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingModel> SettingCollection
        {
            get
            {
                return _settingCollection;
            }
            set
            {
                Set(ref _settingCollection, value);
            }
        }


        #endregion

        #region subregion G-code
        /// <summary>
        /// The <see cref="FeedRate" /> property's name.
        /// </summary>
        public const string FeedRatePropertyName = "FeedRate";
        /// <summary>
        /// Gets the FeedRate property. FeedRate is the motion speed F
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FeedRate
        {
            get
            {
                return _feedRate;
            }
            set
            {
                Set(ref _feedRate, value);
                if (_feedRate < 0)
                {
                    _feedRate = 0;
                }
                if (_feedRate > MaxFeedRate)
                {
                    _feedRate = MaxFeedRate;
                }
                logger.Info("Manual speed rate value is {0}", _feedRate);
            }
        }

        /// <summary>
        /// Sets the maximum feed rate allowed
        /// </summary>
        public double MaxFeedRate { get; private set; } = 1000;

        /// <summary>
        /// The <see cref="Step" /> property's name.
        /// </summary>
        public const string StepPropertyName = "Step";
        /// <summary>
        /// Gets the Step property. Step is the motion step
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Step
        {
            get
            {
                return _step;
            }
            set
            {
                Set(ref _step, value);
                logger.Info("Manual step value is {0}",value);
            }
        }

        /// <summary>
        /// The <see cref="IsSelectedKeyboard" /> property's name.
        /// </summary>
        public const string IsSelectedKeyboardPropertyName = "IsSelectedKeyboard";
        /// <summary>
        /// Gets the IsSelectedKeyboard property. IsSelectedKeyboard is checkbox Keyboard checked.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelectedKeyboard
        {
            get
            {
                return _isSelectedKeyboard;
            }
            set
            {

                Set(ref _isSelectedKeyboard, value);
                if (_isSelectedKeyboard == true)
                {
                    logger.Info("Keyboard is selected");

                }
                else
                {
                logger.Info("Keyboard is not selected");
            }
            }
        }

        /// <summary>
        /// The <see cref="IsJogEnabled" /> property's name.
        /// </summary>
        public const string IsJogEnabledPropertyName = "IsJogEnabled";
        /// <summary>
        /// Gets the IsJogEnabled property. IsJogEnabled allows/disallows jogging button and keypad.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsJogEnabled
        {
            get
            {
                return _isJogEnabled;
            }
            set
            {
                Set(ref _isJogEnabled, value);
            }
        }
        #endregion

        #region subregion machine status, coordinate and version

        /// <summary>
        /// The <see cref="MachineStatus" /> property's name.
        /// </summary>
        public const string MachineStatusPropertyName = "MachineStatus";
        /// <summary>
        /// Gets the MachineStatus property. This is the current state of the machine (Idle, Run, Hold, Alarm...)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MachStatus MachineStatus
        {
            get
            {
                return _machineStatus;
            }
            set
            {
                Set(ref _machineStatus, value);
            }
        }

        /// <summary>
        /// The <see cref="ResponseStatus" /> property's name.
        /// </summary>
        public const string ResponseStatusPropertyName = "ResponseStatus";
        /// <summary>
        /// Gets the ResponseStatus property. This is the current status of the software (Queued, Data received, Ok, Not Ok).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public RespStatus ResponseStatus
        {
            get { return _responseStatus; }
            set { Set(ref _responseStatus, value); }
        }

        /// <summary>
        /// The <see cref="MachineStatusColor" /> property's name.
        /// </summary>
        public const string MachineStatusColorPropertyName = "MachineStatusColor";
        /// <summary>
        /// Gets the MachineStatusColor property. The color change depending of the current state of the machin (Idle=Beige, Run=Light Green...)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush MachineStatusColor
        {
            get
            {
                return _machineStatusColor;
            }
            set
            {
                Set(ref _machineStatusColor, value);
            }
        }

        /// <summary>
        /// The <see cref="VersionGrbl" /> property's name.
        /// </summary>
        public const string VersionPropertyName = "Version";
        /// <summary>
        /// Gets the Version property. This is the Grbl version get w/ '$I' command (0.9i or 1.1j)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string VersionGrbl
        {
            get
            {
                return _versionGrbl;
            }
            set
            {
                Set(ref _versionGrbl, value);
            }
        }

        /// <summary>
        /// The <see cref="Buid" /> property's name.
        /// </summary>
        public const string BuildPropertyName = "Build";
        /// <summary>
        /// Gets the Build property. This is the Grbl date of build information get w/ '$I' command (20150621).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string BuildInfo
        {
            get
            {
                return _buildInfo;
            }
            set
            {
                Set(ref _buildInfo, value);
            }
        }

        /// <summary>
        /// The <see cref="PosX" /> property's name.
        /// </summary>
        public const string PosXPropertyName = "PosX";
        /// <summary>
        /// Gets the PosX property. PosX is the X coordinate of the machin get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PosX
        {
            get
            {
                return _posX;
            }
            set
            {
                Set(ref _posX, value);
            }
        }

        /// <summary>
        /// The <see cref="PosY" /> property's name.
        /// </summary>
        public const string PosYPropertyName = "PosY";
        /// <summary>
        /// Gets the PosY property. PosY is the Y coordinate received the machin get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PosY
        {
            get
            {
                return _posY;
            }
            set
            {
                Set(ref _posY, value);
            }
        }

        /// <summary>
        /// The <see cref="PosZ" /> property's name.
        /// </summary>
        public const string PosZPropertyName = "PosZ";
        /// <summary>
        /// Gets the PosZ property. PosZ is the Z coordinate of the machin get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PosZ
        {
            get
            {
                return _posZ;
            }
            set
            {
                Set("PosZ", ref _posZ, value);
            }
        }
        #endregion
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            try
            {
                //logger.Log(LogLevel.Info, "---Program started ! ---");
                logger.Info("---Program started ! ---");
                _dataService = dataService;
                _dataService.GetData(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("Exception GetData raised: " + error);
                            return;
                        }
                        _serialPort = new SerialPort();
                        GroupBoxPortSettingsTitle = item.PortSettingsHeader;
                        GetSerialPortSettings(item);
                        DefaultPortSettings();
                        MyCommands();
                        InitializeDispatcherTimer();
                        logger.Info("Main MainWindow initialized");
                    });
            }
            catch (Exception ex)
            {
                logger.Error("Exception MainViewModel raised: " + ex.ToString());
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// List of RelayCommands bind to button in ViewModels
        /// </summary>
        private void MyCommands()
        {
            ConnectCommand = new RelayCommand(OpenSerialPort, CanExecuteOpenSerialPort);
            DisconnectCommand = new RelayCommand(CloseSerialPort, CanExecuteCloseSerialPort);
            SendCommand = new RelayCommand(SendData, CanExecuteSendData);
            ClearCommand = new RelayCommand(ClearData, CanExecuteClearData);
            GrblResetCommand = new RelayCommand(GrblReset, CanExecuteGrblReset);
            GrblPauseCommand = new RelayCommand(GrblFeedHold, CanExecuteFeedHold);
            GrblCurrentStatusCommand = new RelayCommand(GrblCurrentStatus, CanExecuteGrblCurrentStatus);
            GrblStartCommand = new RelayCommand(GrblStartCycle, CanExecuteGrblStartCycle);
            GrblSettingsCommand = new RelayCommand(GrblSettings, CanExecuteGrblSettings);
            GrblParametersCommand = new RelayCommand(GrblParameters, CanExecuteGrblParameters);
            GrblParserStateCommand = new RelayCommand(GrblParserState, CanExecuteGrblParserState);
            GrblBuildInfoCommand = new RelayCommand(GrblBuildInfo, CanExecuteGrblBuildInfo);
            GrblStartupBlocksCommand = new RelayCommand(GrblStartupBlocks, CanExecuteGrblStartupBlocks);
            GrblCheckCommand = new RelayCommand(GrblCheck, CanExecuteGrblCheck);
            GrblKillAlarmCommand = new RelayCommand(GrblKillAlarm, CanExecuteGrblKillAlarm);
            GrblHomingCommand = new RelayCommand(GrblHoming, CanExecuteGrblHoming);
            GrblSleepCommand = new RelayCommand(GrblSleep, CanExecuteGrblSleep);
            GrblTestCommand = new RelayCommand(GrblTest, CanExecuteGrblTest);
            GrblHelpCommand = new RelayCommand(GrblHelp, CanExecuteGrblHelp);
            JogHCommand = new RelayCommand<bool>(JogH, CanExecuteJog);//Not update with keyboard
            JogNCommand = new RelayCommand<bool>(JogN, CanExecuteJog);
            JogSCommand = new RelayCommand<bool>(JogS, CanExecuteJog);
            JogECommand = new RelayCommand<bool>(JogE, CanExecuteJog);
            JogWCommand = new RelayCommand<bool>(JogW, CanExecuteJog);
            JogNWCommand = new RelayCommand<bool>(JogNW, CanExecuteJog);
            JogNECommand = new RelayCommand<bool>(JogNE, CanExecuteJog);
            JogSWCommand = new RelayCommand<bool>(JogSW, CanExecuteJog);
            JogSECommand = new RelayCommand<bool>(JogSE, CanExecuteJog);
            JogUpCommand = new RelayCommand<bool>(JogUp, CanExecuteJog);
            JogDownCommand = new RelayCommand<bool>(JogDown, CanExecuteJog);
            StepCommand = new RelayCommand<string>(GetStep,CanExecuteGetStep);
            IncreaseFeedRateCommand = new RelayCommand<bool>(IncreaseFeedRate, CanExecuteFeedRate);
            DecreaseFeedRateCommand = new RelayCommand<bool>(DecreaseFeedRate, CanExecuteFeedRate);
            ResetAxisXCommand = new RelayCommand(ResetAxisX, CanExecuteResetAxis);
            ResetAxisYCommand = new RelayCommand(ResetAxisX, CanExecuteResetAxis);
            ResetAxisZCommand = new RelayCommand(ResetAxisY, CanExecuteResetAxis);
            ResetAllAxisCommand = new RelayCommand(ResetAxisZ, CanExecuteResetAxis);
            logger.Info("All RelayCommands loaded");
        }

        #region subregion serial port method
        /// <summary>
        /// Gets serial port settings from SerialPortSettingsModel class
        /// </summary>
        /// <param name="settingsInit"></param>
        public void GetSerialPortSettings(SerialPortSettingsModel settingsInit)
        {
            try
            {
                ListPortNames = settingsInit.ListPortNames;
                ListBaudRates = settingsInit.ListBaudRates;
                ListDataBits = settingsInit.ListDataBits;
                ListParities = settingsInit.ListParities();
                ListStopBits = settingsInit.ListStopBits();
                ListHandshake = settingsInit.ListHandshake();
                logger.Info("All serial port settings loaded");

            }
            catch (Exception ex)
            {
                logger.Error("Exception GetSerialPortSettings raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Set default settings for serial port
        /// </summary>
        public void DefaultPortSettings()
        {
            try
            {
                if (ListPortNames != null && ListPortNames.Length > 0)
                {
                    _selectedDevicePortName = ListPortNames[0];
                    _selectedBaudRate = 115200;
                    _selectedDataBits = 8;
                    _selectedParity = Parity.None;
                    _selectedStopBits = StopBits.One;
                    _selectedHandshake = Handshake.None;
                    logger.Info("Default settings loaded");
                }
                else
                {
                    logger.Info("No port COM available");
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "Exception DefaultPortSettings raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Starts serial port communication
        /// </summary>
        public void OpenSerialPort()
        {
            try
            {
                if (String.IsNullOrEmpty(SelectedPortName))
                {
                    MessageBox.Show("Please select a Port COM");
                    logger.Info("Select a Port COM");
                }
                else
                {
                    _serialPort.PortName = SelectedPortName;
                    _serialPort.BaudRate = SelectedBaudRate;
                    _serialPort.Parity = SelectedParity;
                    _serialPort.StopBits = SelectedStopBits;
                    _serialPort.DataBits = SelectedDataBits;
                    _serialPort.ReadBufferSize = 100;
                    _serialPort.WriteBufferSize = 100;
                    _serialPort.ReceivedBytesThreshold = 10;
                    _serialPort.DiscardNull = false;
                    _serialPort.DataReceived += _serialPort_DataReceived;
                    _serialPort.Open();
                    GrblReset(); //Do a soft reset before starting a new job
                    GrblBuildInfo();//To get Grbl version information
                    logger.Info("Port COM open");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception OpenSerialPort raised: " + ex.ToString());
            }
        }
        /// <summary>
        /// Allow/Disallow OpenSerialPort method to be executed.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteOpenSerialPort()
        {
            return !_serialPort.IsOpen;
        }

        /// <summary>
        /// Ends serial port communication
        /// </summary>
        public void CloseSerialPort()
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
                logger.Error("Exception CloseSerialPort raised: " + ex.ToString());
            }
        }
        /// <summary>
        /// Allow/Disallow CloseSerialPort method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteCloseSerialPort()
        {
            return _serialPort.IsOpen;
        }

        /// <summary>
        /// Sends G-code or Grb data (TXLine in manual Send data group box) to serial port.
        /// </summary>
        public void SendData()
        {
            try
            {
                _serialPort.WriteLine(TXLine);
                logger.Info("Data TX: {0}", TXLine);

            }
            catch (Exception ex)
            {
                logger.Error("Exception SendData raised: " + ex.ToString());
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
        /// Writes byte (7 bits) to serial port.
        /// </summary>
        /// <param name="b"></param>
        public void WriteByte(byte b)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Write(new byte[1] { b }, 0, 1);
                    if (b != 63)//Skips current status logger with DispatcherTimer
                    {
                        logger.Info("Method WriteByte: {0}", b);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception WriteByte raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Writes bytes to serial port.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="lengh"></param>
        public void WriteBytes(byte[] buffer, int lengh)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Write(buffer, 0, lengh);
                    logger.Info("Method WriteBytes: {0}", buffer.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception WriteBytes raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Writes a string to serial port
        /// </summary>
        /// <param name="data"></param>
        public void WriteString(string data)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.WriteLine(data);
                    logger.Info("Method WriteString: {0}", data);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception WriteString raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Clears group box Send and Data received + serial port and Grbl buffers
        /// </summary>
        public void ClearData()
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    GrblReset();
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                }
                Thread.Sleep(50);
                RXLine = string.Empty;
                TXLine = string.Empty;
                logger.Info("TXLine/RXLine and buffers erased");
            }
            catch (Exception ex)
            {
                logger.Error("Exception ClearData raised: " + ex.ToString());
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
        #endregion

        #region subregion Grbl commands
        /// <summary>
        /// Writes Grbl real-time command (asci dec 24) or 0x18 (Ctrl-x) for a soft reset
        /// </summary>
        public void GrblReset()
        {
            WriteByte(24);
        }
        public bool CanExecuteGrblReset()
        {
            return true;
        }

        /// <summary>
        /// Sends the Grbl '$$' command to get all particular $x=var settings of the machine
        /// </summary>
        public void GrblSettings()
        {
            ListSettingModel.Clear();
            WriteString("$$");
            Thread.Sleep(100);//Waits for ListSettingModel to populate all setting values
            SettingCollection = new ObservableCollection<SettingModel>(ListSettingModel);
        }
        public bool CanExecuteGrblSettings()
        {
            return true;
        }

        /// <summary>
        /// Writes Grbl "~" real-time command (ascii dec 126) to start the machine after a pause or 'M0' command.
        /// </summary>
        public void GrblStartCycle()
        {
            WriteByte(126);
        }
        public bool CanExecuteGrblStartCycle()
        {
            return true;
        }

        /// <summary>
        /// Writes Grbl "!" real-time command (ascii dec 33) to pause the machine motion X, Y and Z (not spindle or laser).
        /// </summary>
        public void GrblFeedHold()
        {
            WriteByte(33);
        }
        public bool CanExecuteFeedHold()
        {
            return true;
        }

        /// <summary>
        /// Writes Grbl "?" real-time command "?" (Ascii dec 63) to get immadiate status report of the machine.
        /// </summary>
        public void GrblCurrentStatus()
        {
            WriteByte(63);
        }
        public bool CanExecuteGrblCurrentStatus()
        {
            return true;
        }

        /// <summary>
        /// Writes Grbl '$#' command to view parameters [G54:0.000,0.000,0.000].
        /// </summary>
        public void GrblParameters()
        {
            WriteString("$#");
        }
        public bool CanExecuteGrblParameters()
        {
            return true;
        }

        /// <summary>
        /// Writes Grbl '$G' command to view G-code parser state [G0 G54 G17 G21 G90 G94 M0 M5 M9 T0 F0. S0.].
        /// </summary>
        public void GrblParserState()
        {
            WriteString("$G");
        }
        public bool CanExecuteGrblParserState()
        {
            return true;
        }

        /// <summary>
        /// Writes Grbl '$I' command to view version and date of build [0.9i.20150620:].
        /// </summary>
        public void GrblBuildInfo()
        {
            WriteString("$I");
        }
        public bool CanExecuteGrblBuildInfo()
        {
            return true;
        }

        /// <summary>
        /// Write Grbl '$N' command to view startup blocks $N0=.
        /// </summary>
        public void GrblStartupBlocks()
        {
            WriteString("$N");
        }
        public bool CanExecuteGrblStartupBlocks()
        {
            return true;
        }

        /// <summary>
        /// Write Grbl '$C' command to enable check mode (No motion).
        /// </summary>
        public void GrblCheck()
        {
            WriteString("$C");
        }
        public bool CanExecuteGrblCheck()
        {
            return true;
        }

        /// <summary>
        /// Write Grbl '$X' command to kill alarm mode.
        /// </summary>
        public void GrblKillAlarm()
        {
            WriteString("$X");
        }
        public bool CanExecuteGrblKillAlarm()
        {
            return true;
        }

        /// <summary>
        /// Write Grbl '$H' command to run homing cycle.
        /// </summary>
        public void GrblHoming()
        {
            WriteString("$H");
        }
        public bool CanExecuteGrblHoming()
        {
            return true;
        }

        /// <summary>
        /// Write Grbl '$SLP' command to enable sleep mode
        /// </summary>
        public void GrblSleep()
        {
            WriteString("$SLP");
        }
        public bool CanExecuteGrblSleep()
        {
            return true;
        }

        /// <summary>
        /// Write Grbl '$' command to get help.
        /// </summary>
        public void GrblHelp()
        {
            WriteString("$");
        }
        public bool CanExecuteGrblHelp()
        {
            return true;
        }
        #endregion

        #region subregion G-code commands
        /// <summary>
        /// Rapid move to home position.
        /// </summary>
        public void JogH(bool parameter)
        {
            string line = Gcode.FormatGcode(0,0, 0, 0, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogH: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Y+
        /// </summary>
        public void JogN(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, 0, 1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogN: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Y-
        /// </summary>
        public void JogS(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, 0, -1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogS: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X+
        /// </summary>
        public void JogE(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, 1, 0, 0, FeedRate, Double.Parse(Step.Replace('.',',')));
            WriteString(line);
            logger.Info("JogE: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X-
        /// </summary>
        public void JogW(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, -1, 0, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogW: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X- Y+
        /// </summary>
        public void JogNW(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, -1, 1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogNW: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X+ Y+
        /// </summary>
        public void JogNE(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, 1, 1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogNE: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X- Y-
        /// </summary>
        public void JogSW(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, -1, -1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogSW: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X+ Y-
        /// </summary>
        public void JogSE(bool parameter)
        {
            string line = Gcode.FormatGcode(1,1, 1, -1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogSE: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Z+
        /// </summary>
        public void JogUp(bool parameter)
        {
            string line = Gcode.FormatGcode(1,0, 0, 0, 1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogUp: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Z-
        /// </summary>
        public void JogDown(bool parameter)
        {
            string line = Gcode.FormatGcode(1,0, 0, 0, -1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("JogDown: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Allows jogging mode if keyboard is not checked.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteJog(bool parameter)
        {
            if(parameter) //&&_serialPort.IsOpen;
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the move step value
        /// </summary>
        /// <param name="parameter"></param>
        private void GetStep(string parameter)
        {
            Step = (string)parameter;
        }

        /// <summary>
        /// Allows/Disallows GetStep method.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteGetStep(string parameter)
        {
            if(parameter!=null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the move feed rate value
        /// </summary>
        /// <param name="parameter"></param>
        private void IncreaseFeedRate(bool parameter)
        {
            FeedRate+=10;
        }

        /// <summary>
        /// Gets the move feed rate value
        /// </summary>
        /// <param name="parameter"></param>
        private void DecreaseFeedRate(bool parameter)
        {
            FeedRate-=10;
        }

        /// <summary>
        /// Allows/Disallows GetFeedRate method.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteFeedRate(bool parameter)
        {
            if (!parameter&&FeedRate<0&&FeedRate>MaxFeedRate)
            {
                RXLine = "false";
                return false;
            }
            RXLine = "true";
            return true;
        }

        /// <summary>
        /// Sets current axis X to 0.
        /// </summary>
        public void ResetAxisX()
        {
            //string line = "G10 P0 L20 X0";
            string line = "G10 P0 L2 X0";
            WriteString(line);
            logger.Info("Reset X: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Sets current axis Y to 0.
        /// </summary>
        public void ResetAxisY()
        {
            string line = "G10 P0 L20 Y0";
            WriteString(line);
            logger.Info("Reset Y: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Sets current axis Z to 0.
        /// </summary>
        public void ResetAxisZ()
        {
            string line = "G10 P0 L20 Z0";
            WriteString(line);
            logger.Info("Reset Z: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Sets all current axis to 0.
        /// </summary>
        public void ResetAllAxis()
        {
            string line = "G10 P0 L20 X0 Y0 Z0";
            WriteString(line);
            logger.Info("Reset All axis: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Allows/Disallows Reset axis. G10 command only available w/ 0.9j and above?
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteResetAxis()
        {
            if (VersionGrbl.Contains("1"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        /// <summary>
        /// This is a test command bind to TEST button for development purpose only.
        /// </summary>
        public void GrblTest()
        {
            //TODO Test
        }
        public bool CanExecuteGrblTest()
        {
            return true;
        }

        /// <summary>
        /// Sorts Grbl data received like Grbl informations, response, coordinates, settings...
        /// </summary>
        /// <param name="line"></param>
        public void DataGrblSorter(string _line)
        {
            string line = FormatGrblLine(_line);
            try
            {
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.StartsWith("ok"))
                    {
                        ProcessResponse(line);
                    }
                    else if (line.StartsWith("error"))
                    {
                        ProcessErrorResponse(line);
                    }
                    else if (line.StartsWith("alarm"))
                    {
                        ProcessAlarmResponse(line);
                    }
                    else if (line.StartsWith("<") && line.EndsWith(">"))
                    {
                        ProcessCurrentStatusResponse(line);
                    }
                    else if (line.StartsWith("$")&&line.Contains("="))
                    {
                        ProcessSettingsResponse(line);
                    }
                    else if(line.StartsWith("[") && line.EndsWith("]"))
                    {
                        ProcessInfoResponse(line);
                    }
                    else
                    {
                        ResponseStatus = RespStatus.Q;
                        logger.Info("Data:{0}|RespStatus:{1}|MachStatus:{2}", line, ResponseStatus.ToString(), MachineStatus.ToString());
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error("Exception DataGrblSorter raised: " + ex.ToString());
            }
        }

        #region process methods for DataGrblSorter method
        /// <summary>
        /// Processes Grbl build informations.
        /// </summary>
        /// <param name="_data"></param>
        public void ProcessInfoResponse(string _data)
        {
            try
            {
                if(_data.Length==16)
                {
                    VersionGrbl = _data.Substring(1, 4);
                    BuildInfo = _data.Substring(6, 8);
                    ResponseStatus = RespStatus.Ok;
                    logger.Info("Data:{0}|RespStatus:{1}|MachStatus:{2}", _data, ResponseStatus.ToString(), MachineStatus.ToString());
                }
            }
            /*List of [] message
             Grbl vX.Xx ['$' for help]
             [0.9i.20150620:]
             [Reset to continue]
             ['$H'|'$X' to unlock]
             [Caution: Unlocked]
             [Enabled]
             [Disabled]
             [PRB:0.000,0.000,1.492:1]
             [G0 G54 G17 G21 G90 G94 M0 M5 M9 T0 F0. S0.]*/
            catch (Exception ex)
            {
                logger.Error("Exception ProcessInfoResponse raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Processes the serial port ok message reply.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_isError"></param>
        /// <returns></returns>
        public void ProcessResponse(string _data)
        {
            ResponseStatus = RespStatus.Ok;
            logger.Info("Data:{0}|RespStatus:{1}|MachStatus:{2}", _data, ResponseStatus.ToString(), MachineStatus.ToString());
        }

        /// <summary>
        /// Processes the serial port error message reply.
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public string ProcessErrorResponse(string _data)//Should not return a string but update a property TODO.
        {
            //ResponseStatus = RespStatus.Ok;//It is an error but still ok to send next command + try/catch
                ErrorCodes ec = new ErrorCodes();
                logger.Info("Error key:{0}", _data.Split(':')[1]);
                if (VersionGrbl.StartsWith("1"))//In version 1.1 all error codes have ID
                {
                    string errorDesc11 = ec.ErrorDict11[_data.Split(':')[1]];
                    logger.Info("Error key:{0} | description:{1}", _data.Split(':')[1], errorDesc11);
                    return errorDesc11;
                }
                else
                {
                    if (_data.Contains("ID"))//In version 0.9 only error code from 23 to 37 have ID
                    {
                        string errorDesc09 = ec.ErrorDict09[_data.Split(':')[2]];
                        logger.Info("Error key {0} | description:{1}", _data.Split(':')[2], errorDesc09);
                        return errorDesc09;
                    }
                    else//Error codes w/o ID
                    {
                        string errorDesc09 = ec.ErrorDict09[_data.Split(':')[1]];
                        logger.Info("Error key {0} | description:{1}", _data.Split(':')[1], errorDesc09);
                        return errorDesc09;
                    }
                }
        }

        /// <summary>
        /// Processes the serial port alarm message reply.
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public string ProcessAlarmResponse(string _data)//Should not return a string but update a property TODO.
        {
            //TODO: what to do before setting ResponseStatus to Ok...
            ResponseStatus = RespStatus.NOk;
            logger.Info("Data:{0}|RespStatus:{1}|MachStatus{2}", _data, ResponseStatus.ToString(), MachineStatus.ToString());
            try
            {
                AlarmCodes ac = new AlarmCodes();
                return ac.AlarmDict11[_data.Split(':')[1]];
            }
            catch(Exception ex)
            {
                logger.Error("Exception ProcessAlarmResponse raised: ", ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// Populates the settingsCollection w/ data received w/ Grbl '$$' command.
        /// </summary>
        /// <param name="_data"></param>
        public void ProcessSettingsResponse(string _data)
        {
            try
            {
                string[] arr = _data.Split(new Char[] { '=', '(', ')', '\r', '\n' });
                
                if (arr.Length > 2)//Grbl version 0.9 (w/ setting description)
                {
                    _settingModel = new SettingModel(arr[0], arr[1], arr[2]);
                    ListSettingModel.Add(_settingModel);
                    ResponseStatus = RespStatus.Ok;
                }
                else//Grbl version 1.1 (w/o setting description)
                {
                    _settingModel = new SettingModel(arr[0], arr[1], "");
                    ListSettingModel.Add(_settingModel);
                    ResponseStatus = RespStatus.Ok;
                }
                logger.Info("Setting Value:{0}|RespStatus:{1}|MachStatus{2}", _data, ResponseStatus.ToString(), MachineStatus.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("Exception ProcessSettingResponse raised: ", ex.ToString());
            }
        }

        /// <summary>
        /// Gets machine coordinates and status depending of Grbl version.
        /// </summary>
        /// <param name="_data"></param>
        public void ProcessCurrentStatusResponse(string _data)
        {
            if (_data.Contains("|") || VersionGrbl.StartsWith("1"))//Report state Grbl v1.1 < Idle|MPos:0.000,0.000,0.000>
            {
                string[] arr = _data.Split(new Char[] { '<', '>', ',', ':', '\r', '\n', '|' });
                PosX = arr[3];
                PosY = arr[4];
                PosZ = arr[5];
                switch (arr[1])
                {
                    case "idle":
                        MachineStatus = MachStatus.Idle;
                        MachineStatusColor = Brushes.Beige;
                        break;
                    case "run":
                        MachineStatus = MachStatus.Run;
                        MachineStatusColor = Brushes.LightGreen;
                        break;
                    case "hold":
                        MachineStatus = MachStatus.Hold;
                        MachineStatusColor = Brushes.LightBlue;
                        break;
                    case "alarm":
                        MachineStatus = MachStatus.Alarm;
                        MachineStatusColor = Brushes.Red;
                        break;
                    case "jog":
                        MachineStatus = MachStatus.Jog;
                        MachineStatusColor = Brushes.LightSeaGreen;
                        break;
                    case "door":
                        MachineStatus = MachStatus.Door;
                        MachineStatusColor = Brushes.LightYellow;
                        break;
                    case "check":
                        MachineStatus = MachStatus.Check;
                        MachineStatusColor = Brushes.LightCyan;
                        break;
                    case "home":
                        MachineStatus = MachStatus.Home;
                        MachineStatusColor = Brushes.LightPink;
                        break;
                    case "sleep":
                        MachineStatus = MachStatus.Sleep;
                        MachineStatusColor = Brushes.LightGray;
                        break;
                }
            }
            else//Report state Grbl v0.9 <Idle,MPos:0.000,0.000,0.000,WPos:0.000,0.000,0.000>
            {
                string[] arr = _data.Split(new Char[] { '<', '>', ',', ':', '\r', '\n' });
                PosX = arr[3];
                PosY = arr[4];
                PosZ = arr[5];
                switch (arr[1])
                {
                    case "idle":
                        MachineStatus = MachStatus.Idle;
                        MachineStatusColor = Brushes.Beige;
                        break;
                    case "run":
                        MachineStatus = MachStatus.Run;
                        MachineStatusColor = Brushes.LightGreen;
                        break;
                    case "hold":
                        MachineStatus = MachStatus.Hold;
                        MachineStatusColor = Brushes.LightBlue;
                        break;
                    case "alarm":
                        MachineStatus = MachStatus.Alarm;
                        MachineStatusColor = Brushes.Red;
                        break;
                    case "door":
                        MachineStatus = MachStatus.Door;
                        MachineStatusColor = Brushes.LightYellow;
                        break;
                    case "check":
                        MachineStatus = MachStatus.Check;
                        MachineStatusColor = Brushes.LightCyan;
                        break;
                    case "home":
                        MachineStatus = MachStatus.Home;
                        MachineStatusColor = Brushes.LightPink;
                        break;
                }
            }
            //logger.Info("Current state:{0}|RespStatus:{1}|MachStatus:{2}|Color:{3}", _data, ResponseStatus.ToString(), MachinStatus.ToString(), MachinStatusColor.ToString());
        }
        #endregion

        #region subregion other methods
        /// <summary>
        /// Elimantes 'space', '\r', '\n', of Grbl response line
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public string FormatGrblLine(string _data)
        {
            return _data.ToLower().Trim().TrimEnd(trimArray);
        }

        /// <summary>
        /// Does nothing yet...
        /// </summary>
        public override void Cleanup()
        {
            // Clean up if needed

            base.Cleanup();
        }

        /// <summary>
        /// Initializes DispatcherTimer to query Grbl report state at 4Hz (5Hz is max recommended).
        /// </summary>
        private void InitializeDispatcherTimer()
        {
            DispatcherTimer currentStatusTimer = new DispatcherTimer(DispatcherPriority.Normal);
            currentStatusTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            //dispatcherTimer.Interval = TimeSpan.FromSeconds(0.25);
            currentStatusTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            currentStatusTimer.Start();
            logger.Info("Initialize Dispatcher Timer");
        }
        #endregion
        #endregion

        #region Event
        /// <summary>
        /// Gets all data from serial port and print it in data received group box except report state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = _serialPort.ReadLine();
                if (!line.StartsWith("<"))
                {
                    RXLine += line + Environment.NewLine;
                }
                DataGrblSorter(line);
            }
            catch (Exception ex)
                {
                logger.Error("Exception data received raised: " + ex.ToString());
                _responseStatus = RespStatus.Q;
            }
        }

        /// <summary>
        /// Queries report state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            GrblCurrentStatus();
        }
        #endregion

        #region IDisposable Support
        /// <summary>
        /// Implements the method for IDispose interface
        /// Disposes the serial communication
        /// </summary>
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ((IDisposable)_serialPort).Dispose();
                    logger.Info("Port COM disposed");
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        // ~MainViewModel() {
        //   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
        //   Dispose(false);
        // }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose(true);
            // TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}