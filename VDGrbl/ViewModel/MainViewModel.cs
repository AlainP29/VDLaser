using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VDGrbl.Model;
using VDGrbl.Tools;
using VDGrbl.Service;
using System.Globalization;
using System.Diagnostics;

namespace VDGrbl.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// Currently it is the common ViewModel for all views.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        #region private Fields

        private int _selectedBaudRate;
        private int transferDelay;
        private int _selectedLaser;
        private int _converterParameterLaser;

        private string _selectedPortName = string.Empty;
        private string _versionGrbl = "-", _buildInfoGrbl = "-";
        private string _mposX = "0.000", _mposY = "0.000";
        private string _wposX = "0.000", _wposY = "0.000";
        private string _feed = "0", _speed = "0";
        private string _step = "1";
        private string _buf = "0", _rx = "0";
        private string _errorMessage = string.Empty;
        private string _alarmMessage = string.Empty;
        private string _infoMessage = string.Empty;
        private string _fileName = string.Empty;
        private string _estimateJobTime = "00:00:00";
        private string _groupBoxPortSettingTitle = string.Empty;
        private string _groupBoxGrblConsoleTitle = string.Empty;
        private string _groupBoxGCodeFileTitle = string.Empty;
        private string _groupBoxConsoleTitle = string.Empty;
        private string _groupBoxGraphicTitle = string.Empty;
        private string _groupBoxJoggingTitle = string.Empty;
        private string _selectedTransferDelay = string.Empty;
        private string _txLine = string.Empty;
        private string _rxLine = string.Empty;
        private string _gcodeLine = string.Empty;
        private string _macro1 = "G90 G0 X0", _macro2 = "G91 G1 X-100 Y-100 F1000", _macro3 = "G90 G1 X50 Y50 F5000", _macro4 = "G91 G1 X-50 Y50 F1000";

        private double _manualfeedRate = 300;
        private double _nLine = 0;
        private double _rLine = 0;
        private double _percentLine = 0;
        private double _laserPower = 0;
        private double _maxLaserPower = 2500;
        private double _strokeThickness = 2;

        private bool _isSelectedKeyboard = false;
        private bool _isSelectedMetric = true;
        private bool _isJogEnabled = true;
        private bool _isVerbose = false;
        private bool _isManualSending = true;
        private bool _isSending = false;
        private bool _isPortEnabled = true;
        private bool _isBaudEnabled = true;
        private bool _isLaserEnabled = false;
        private bool _isRefresh = false;

        private Collection<string> _collectionPortName;
        private readonly IDataService _dataService;
        private PointCollection _gcodePoints= new PointCollection();
        private PathGeometry _pathGeometry;
        private Brush _fill=Brushes.AliceBlue;
        private Brush _stroke=Brushes.White;
        private SerialPort _serialPort;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly DispatcherTimer currentStatusTimer = new DispatcherTimer(DispatcherPriority.Normal);
        private CancellationTokenSource cts;
        private RespStatus _responseStatus = RespStatus.Ok;
        private MachStatus _machineStatus = MachStatus.Idle;
        private SolidColorBrush _machineStatusColor = new SolidColorBrush(Colors.LightGray);
        private SolidColorBrush _laserColor = new SolidColorBrush(Colors.LightGray);
        private ObservableCollection<SettingItem> _settingCollection=new ObservableCollection<SettingItem>();
        private Queue<string> _fileQueue = new Queue<string>();
        private List<string> _fileList = new List<string>();
        private ObservableCollection<ConsoleModel> _consoleData;
        private ObservableCollection<GCodeModel> _gcodeData;
        private ObservableCollection<GraphicItems> _paths=new ObservableCollection<GraphicItems>();
        private List<ConsoleModel> _listConsoleData = new List<ConsoleModel>();
        private ConsoleModel _console = new ConsoleModel("TX", "RX");
        private GCodeModel _gcodeModel;
        private readonly GrblTool grbltool = new GrblTool();
        private GCodeTool gcodeTool;
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);//Use to monitor file sending

        #region subregion enum
        /// <summary>
        /// Enumeration of the response states. Ok: All is good, NOk: Alarm state Q: Queued [DR: Data received] 
        /// </summary>
        public enum RespStatus { Ok, NOk, Q };

        /// <summary>
        /// Enumeration of the machine states:
        /// (V0.9) Idle, Run, Hold, Door, Home, Alarm, Check
        /// (V1.1) Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep
        /// </summary>
        public enum MachStatus { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep };
        #endregion
        #endregion

        #region public Properties
        #region subregion Relaycommands
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        public RelayCommand RefreshPortCommand { get; private set; }
        public RelayCommand SendManualCommand { get; private set; }
        public RelayCommand SendM1Command { get; private set; }
        public RelayCommand SendM2Command { get; private set; }
        public RelayCommand SendM3Command { get; private set; }
        public RelayCommand SendM4Command { get; private set; }
        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand GrblResetCommand { get; private set; }
        public RelayCommand GrblPauseCommand { get; private set; }
        public RelayCommand GrblCurrentStatusCommand { get; private set; }
        public RelayCommand GrblStartCommand { get; private set; }
        public RelayCommand GrblRefreshSettingsCommand { get; private set; }
        public RelayCommand GrblParametersCommand { get; private set; }
        public RelayCommand GrblParserStateCommand { get; private set; }
        public RelayCommand GrblBuildInfoCommand { get; private set; }
        public RelayCommand GrblStartupBlocksCommand { get; private set; }
        public RelayCommand GrblCheckCommand { get; private set; }
        public RelayCommand GrblKillAlarmCommand { get; private set; }
        public RelayCommand GrblHomingCommand { get; private set; }
        public RelayCommand GrblHome1Command { get; private set; }
        public RelayCommand GrblSetHome1Command { get; private set; }
        public RelayCommand GrblHome2Command { get; private set; }
        public RelayCommand GrblSetHome2Command { get; private set; }
        public RelayCommand GrblSleepCommand { get; private set; }
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
        public RelayCommand<bool> DecreaseFeedRateCommand { get; private set; }
        public RelayCommand<bool> IncreaseFeedRateCommand { get; private set; }
        public RelayCommand ResetAxisXCommand { get; private set; }
        public RelayCommand ResetAxisYCommand { get; private set; }
        public RelayCommand StartLaserCommand { get; private set; }
        public RelayCommand StopLaserCommand { get; private set; }
        public RelayCommand LoadFileCommand { get; private set; }
        public RelayCommand SendFileCommand { get; private set; }
        public RelayCommand PauseFileCommand { get; private set; }
        public RelayCommand StopFileCommand { get; private set; }
        public RelayCommand<bool> DecreaseLaserPowerCommand { get; private set; }
        public RelayCommand<bool> IncreaseLaserPowerCommand { get; private set; }
        #endregion

        #region subregion setting
        /// <summary>
        /// Get the ListGrblSettings property. ListGrblSettings is populated w/ Grbl settings data ('$$' command)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// First get ListGrblSettings then populate the settingCollection (observableCollection) for binding.
        /// </summary>
        public List<SettingItem> ListGrblSettings { get; set; } = null;

        /// <summary>
        /// Gets the SettingCollection property. SettingCollection is populated w/ data from ListSettingModel
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingItem> SettingCollection
        {
            get
            {
                return _settingCollection;
            }
            set
            {
                Set(ref _settingCollection, value);
                logger.Info("MainWindowModel|SettingCollection");
            }
        }
        #endregion

        #region subregion console
        /// <summary>
        /// Get the GroupBoxGrblConsoleTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxGrblConsoleTitle
        {
            get
            {
                return _groupBoxGrblConsoleTitle;
            }
            set
            {
                Set(ref _groupBoxGrblConsoleTitle, value);
            }
        }

        /// <summary>
        /// Gets the ListConsoleData property. ListConsoleData is populated w/ TXLine/RXLine
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<ConsoleModel> ListConsoleData
        {
            get
            {
                return _listConsoleData;
            }
            set
            {
                Set(ref _listConsoleData, value);
            }
        }

        /// <summary>
        /// Gets the ConsoleData property. ConsoleData is populated w/ data from ListConsoleData
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ConsoleModel> ConsoleData
        {
            get
            {
                return _consoleData;
            }
            set
            {
                Set(ref _consoleData, value);
            }
        }

        /// <summary>
        /// Gets the ErrorMessage property. ErrorMessage is got from ErrorCode ID.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                Set(ref _errorMessage, value);
                logger.Error(CultureInfo.CurrentCulture, "MainWindowModel|ErrorMessage {0}", value);

            }
        }

        /// <summary>
        /// Gets the AlarmMessage property. AlarmMessage is got from AlarmCode ID.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string AlarmMessage
        {
            get
            {
                return _alarmMessage;
            }
            set
            {
                Set(ref _alarmMessage, value);
                logger.Error(CultureInfo.CurrentCulture, "MainWindowModel|AlarmMessage {0}", value);
            }
        }

        /// <summary>
        /// Gets the InfoMessage property. InfoMessage is got from the Arduino or the VDGrbl software.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string InfoMessage
        {
            get
            {
                return _infoMessage;
            }
            set
            {
                Set(ref _infoMessage, value);
                //logger.Info(CultureInfo.CurrentCulture, "MainWindowModel|InfoMessage {0}", value);
            }
        }

        /// <summary>
        /// Gets the IsVerbose property. Allows/Disallows the verbose mode, especially to print current status.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsVerbose
        {
            get
            {
                return _isVerbose;
            }
            set
            {
                Set(ref _isVerbose, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|IsVerbose {0}", value);

            }
        }
        /// <summary>
        /// Gets the IsRefresh property. if IsRefresh is true load settings is ok.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsRefresh
        {
            get
            {
                return _isRefresh;
            }
            set
            {
                Set(ref _isRefresh, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|IsRefresh {0}", value);
            }
        }
        #endregion

        #region subregion send
        /// <summary>
        /// Get the ConsoleModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ConsoleModel Console
        {
            get
            {
                return _console;
            }
            set
            {
                Set(ref _console, value);
            }
        }

        /// <summary>
        /// Get the GCodeModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GCodeModel GCodeModel
        {
            get
            {
                return _gcodeModel;
            }
            set
            {
                Set(ref _gcodeModel, value);
            }
        }

        /// <summary>
        /// Gets the TXLine property. TXLine is the transmetted G-Code or Grbl commands to the Arduino.
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
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|TXLine {0}", value);
            }
        }

        /// <summary>
        /// Get the Macro property. Macro is a stored G-Code or Grbl commands which can be send manually
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Macro1
        {
            get
            {
                return _macro1;
            }
            set
            {
                Set(ref _macro1, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Macro 1: {0}", value);
            }
        }

        /// <summary>
        /// Get the Macro property. Macro is a stored G-Code or Grbl commands which can be send manually
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Macro2
        {
            get
            {
                return _macro2;
            }
            set
            {
                Set(ref _macro2, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Macro 2: {0}", value);
            }
        }

        /// <summary>
        /// Get the Macro property. Macro is a stored G-Code or Grbl commands which can be send manually
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Macro3
        {
            get
            {
                return _macro3;
            }
            set
            {
                Set(ref _macro3, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Macro 3: {0}", value);
            }
        }

        /// <summary>
        /// Get the Macro property. Macro is a stored G-Code or Grbl commands which can be send manually
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Macro4
        {
            get
            {
                return _macro4;
            }
            set
            {
                Set(ref _macro4, value);
                logger.Info("MainViewModel|Macro 4: {0}", value);
            }
        }

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
        /// Get the IsSendingFile property. When sending file we cannot use jog, send manual or clear command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsManualSending
        {
            get
            {
                return _isManualSending;
            }
            set
            {
                Set(ref _isManualSending, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Manual sending: {0}", value);
            }
        }

        /// <summary>
        /// Gets the FeedRate property. FeedRate is the motion speed F
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FeedRate
        {
            get
            {
                return _manualfeedRate;
            }
            set
            {
                Set(ref _manualfeedRate, value);
                if (_manualfeedRate < 0)
                {
                    _manualfeedRate = 0;
                }
                if (_manualfeedRate > MaxFeedRate)
                {
                    _manualfeedRate = MaxFeedRate;
                }
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Manual speed rate value : {0}", value);
            }
        }

        /// <summary>
        /// Sets the maximum feed rate allowed
        /// </summary>
        public double MaxFeedRate { get; private set; } = 2000;

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
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Manual step value : {0}", value);
            }
        }

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
                logger.Info("MainViewModel|Keyboard is selected: {0}", value);
            }
        }

        /// <summary>
        /// Gets the IsSelectedKeyboard property. IsSelectedKeyboard is checkbox Keyboard checked.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelectedMetric
        {
            get
            {
                return _isSelectedMetric;
            }
            set
            {

                Set(ref _isSelectedMetric, value);
                if (_isSelectedMetric == true && _serialPort.IsOpen)
                {
                    WriteString("G21");
                    logger.Info("MainViewModel|Metric is selected");
                }
                if (_isSelectedMetric == false && _serialPort.IsOpen)
                {
                    WriteString("G20");
                    logger.Info("MainViewModel|Metric is not selected");
                }
            }
        }

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
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Jog is enabled: {0}", value);
            }
        }

        /// <summary>
        /// Gets the ResponseStatus property. This is the current status of the software (Queued, Data received, Ok, Not Ok).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public RespStatus ResponseStatus
        {
            get
            {
                return _responseStatus;
            }
            set
            {
                Set(ref _responseStatus, value);
                //logger.Info("MainViewModel| Response State {}", value);
            }
        }
        #endregion

        #region subregion laser
        /// <summary>
        /// Get the LaserPower property. LaserPower is the power of the laser.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double LaserPower
        {
            get
            {
                return _laserPower;
            }
            set
            {
                Set(ref _laserPower, value);
                if (_laserPower < 0)
                {
                    _laserPower = 0;
                }
                if (_laserPower > MaxLaserPower)
                {
                    _laserPower = MaxLaserPower;
                }
                WriteString(string.Format("S{0}", _laserPower));
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Manual laser power value : {0}", value);
            }
        }

        /// <summary>
        /// Sets the maximum laser power allowed.
        /// </summary>
        public double MaxLaserPower
        {
            get
            {
                return _maxLaserPower;
            }
            set
            {
                Set(ref _maxLaserPower, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Max laser power value : {0}", value);
            }
        }

        /// <summary>
        /// Select the type of laser used.
        /// </summary>
        public int SelectedLaser
        {
            get
            {
                return _selectedLaser;
            }
            set
            {
                Set(ref _selectedLaser, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Selected laser : {0}", value);
                //MaxLaserPower = 500;
                //WriteString("$30=500");
            }
        }

        public int ConverterParameterLaser
        {
            get
            {
                return _converterParameterLaser;
            }
            set
            {
                Set(ref _converterParameterLaser, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|ConverterParameterLaser : {0}", value);
            }
        }

        /// <summary>
    /// Enable/disable laser.
    /// </summary>
        public bool IsLaserEnabled
        {
            get
            {
                return _isLaserEnabled;
            }
            set
            {
                Set(ref _isLaserEnabled, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|IsLaserEnabled : {0}", value);
            }
        }

        /// <summary>
        /// Get the ListLaser property.
        /// </summary>
        public int[] ListLaser { get; private set; } = { 500, 1600, 2500, 5500 };

        /// <summary>
        /// Check laser status.
        /// </summary>
        public bool IsLaserPower { get; private set; } = false;

        /// <summary>
        /// Get the LaserColor property. The color change depending of the current state of the laser (ON=Blue, OFF=Light Gray...)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush LaserColor
        {
            get
            {
                return _laserColor;
            }
            set
            {
                Set(ref _laserColor, value);
            }
        }
        #endregion

        #region subregion G-Code
        /// <summary>
        /// Get the GroupBoxGCodeFileTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxGCodeFileTitle
        {
            get
            {
                return _groupBoxGCodeFileTitle;
            }
            set
            {
                Set(ref _groupBoxGCodeFileTitle, value);
            }
        }

        /// <summary>
        /// Get the FileName property. FileName is the G-code complete path w/ file name.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                Set(ref _fileName, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|File name : {0}", value);
            }
        }

        /// <summary>
        /// Get the EstimateJobTime property. It is the estimation of running time.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string EstimateJobTime
        {
            get
            {
                return _estimateJobTime;
            }
            set
            {
                Set(ref _estimateJobTime, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Job time : {0}", value);
            }
        }

        /// <summary>
        /// Get the FileQueue property. FileQueue is populated w/ lines of G-code file trimed.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Queue<string> FileQueue
        {
            get
            {
                return _fileQueue;
            }
            set
            {
                Set(ref _fileQueue, value);
                logger.Info("MainViewModel|File queued");
            }
        }

        /// <summary>
        /// Get the FileList property. FileList is populated w/ lines of G-code file.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<string> FileList
        {
            get
            {
                return _fileList;
            }
            set
            {
                Set(ref _fileList, value);
                logger.Info("MainViewModel|File listed");
            }
        }

        /// <summary>
        /// Get the NLine property. NLine is the number of lines in the G-code file.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double NLine
        {
            get
            {
                return _nLine;
            }
            set
            {
                Set(ref _nLine, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Total of lines: {0}", value);
            }
        }

        /// <summary>
        /// Get the RLine property. RLine is the number of lines remaining in the G-code queue.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double RLine
        {
            get
            {
                return _rLine;
            }
            set
            {
                Set(ref _rLine, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Number of remaining lines: {0}", value);
            }
        }

        /// <summary>
        /// Get the Buf property. Buf is the number of motions queued in Grbl's planner buffer.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Buf
        {
            get
            {
                return _buf;
            }
            set
            {
                Set(ref _buf, value);
            }
        }

        /// <summary>
        /// Get the RX property. RX is the number of characters queued in Grbl's serial RX receive buffer.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RX
        {
            get
            {
                return _rx;
            }
            set
            {
                Set(ref _rx, value);
            }
        }

        /// <summary>
        /// Get the IsSending property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSending
        {
            get
            {
                return _isSending;
            }
            set
            {
                Set(ref _isSending, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|IsSending {0}", value);
            }
        }

        /// <summary>
        /// Get the PercentLine property. PercentLine is the progress.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double PercentLine
        {
            get
            {
                return _percentLine;
            }
            set
            {
                Set(ref _percentLine, value);
            }
        }

        /// <summary>
        /// Get the SelectedTransferDelay property. This is the delay between two lines.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedTransferDelay
        {
            get
            {
                return _selectedTransferDelay;
            }
            set
            {
                Set(ref _selectedTransferDelay, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Set Transfer delay : {0}", value);
            }
        }

        /// <summary>
        /// Get the ListTransferDelay property. 
        /// Bind to combobox.
        /// </summary>
        public string[] ListTransferDelay { get; private set; } = { "Slow", "Normal", "Fast", "UltraFast" };

        /// <summary>
        /// Gets the GCodeLine property. GCodeLine is the G-Code line displayed.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GCodeLine
        {
            get
            {
                return _gcodeLine;
            }
            set
            {
                Set(ref _gcodeLine, value);
            }
        }
        public ObservableCollection<GCodeModel> GCodeData
        {
            get
            { 
                return _gcodeData;
            }
            set
            { 
                Set(ref _gcodeData, value);
            }
        }
        #endregion

        #region subregion port settings
        /// <summary>
        /// Get the GroupBoxPortSettingsTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxPortSettingTitle
        {
            get
            {
                return _groupBoxPortSettingTitle;
            }
            set
            {
                Set(ref _groupBoxPortSettingTitle, value);
            }
        }

        /// <summary>
        /// Get the SelectedPortName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedPortName
        {
            get
            {
                return _selectedPortName;
            }
            set
            {
                Set(ref _selectedPortName, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Selected Port name: {0}", value);
            }
        }

        /// <summary>
        /// Get the CollectionPortNames property. 
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Collection<string> CollectionPortNames
        {
            get
            {
                return _collectionPortName;
            }
            private set
            {
                Set(ref _collectionPortName, value);
                logger.Info("MainViewModel|Selected Port : {0}", value);
            }
        }

        /// <summary>
        /// Get the SelectedBaudRateName property.
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
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Selected Baudrate : {0}", value);
            }
        }

        /// <summary>
        /// Get the ListBaudRates property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int[] ListBaudRates { get; private set; } = { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400 };

        /// <summary>
        /// Disable/enable combobox port name when serial port is open/close
        /// </summary>
        public bool IsPortEnabled
        {
            get
            {
                return _isPortEnabled;
            }
            set
            {
                Set(ref _isPortEnabled, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|IsPortEnabled: {0}", value);
            }
        }

        /// <summary>
        /// Disable/enable combobox baud rate when serial port is open/close
        /// </summary>
        public bool IsBaudEnabled
        {
            get
            {
                return _isBaudEnabled;
            }
            set
            {
                Set(ref _isBaudEnabled, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|IsBaudEnabled: {0}", value);
            }
        }
        #endregion

        #region subregion grbl info et control
        /// <summary>
        /// Get the Version property. This is the Grbl version get w/ '$I' command (0.9i or 1.1j)
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
        /// Gets the Build property. This is the Grbl date of build information get w/ '$I' command (20150621).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string BuildInfoGrbl
        {
            get
            {
                return _buildInfoGrbl;
            }
            set
            {
                Set(ref _buildInfoGrbl, value);
            }
        }
        #endregion

        #region subregion machine state
        /// <summary>
        /// Get the GroupBoxConsoleTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxConsoleTitle
        {
            get
            {
                return _groupBoxConsoleTitle;
            }
            set
            {
                Set(ref _groupBoxConsoleTitle, value);
            }
        }

        /// <summary>
        /// Get the MachineStatus property. This is the current state of the machine (Idle, Run, Hold, Alarm...)
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
        /// Get the MachineStatusColor property. The color change depending of the current state of the machin (Idle=Beige, Run=Light Green...)
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
        /// Get the MPosX property. MPosX is the X machine coordinate get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MPosX
        {
            get
            {
                return _mposX;
            }
            set
            {
                Set(ref _mposX, value);
            }
        }

        /// <summary>
        /// Get the MPosY property. MPosY is the Y machine coordinate get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MPosY
        {
            get
            {
                return _mposY;
            }
            set
            {
                Set(ref _mposY, value);
            }
        }

        /// <summary>
        /// Get the WPosX property. WPosX is the X work coordinate w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WPosX
        {
            get
            {
                return _wposX;
            }
            set
            {
                Set(ref _wposX, value);
            }
        }

        /// <summary>
        /// Get the WPosY property. WPosY is the Y work coordinate w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WPosY
        {
            get
            {
                return _wposY;
            }
            set
            {
                Set(ref _wposY, value);
            }
        }

        /// <summary>
        /// Get the Feed property. Feed is the real time feed of the machine get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Feed
        {
            get
            {
                return _feed;
            }
            set
            {
                Set(ref _feed, value);
            }
        }

        /// <summary>
        /// Get the Speed property. Speed is the real time spindle speed or laser power of the machine get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                Set(ref _speed, value);
            }
        }

        #endregion

        #region Graphics
        /// <summary>
        /// Get the GroupBoxGraphicTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxGraphicTitle
        {
            get
            {
                return _groupBoxGraphicTitle;
            }
            set
            {
                Set(ref _groupBoxGraphicTitle, value);
            }
        }

        /// <summary>
        /// Get the GroupBoxJoggingTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxJoggingTitle
        {
            get
            {
                return _groupBoxJoggingTitle;
            }
            set
            {
                Set(ref _groupBoxJoggingTitle, value);
            }
        }

        /// <summary>
        /// Get the GCodePoints property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PointCollection GCodePoints
        {
            get
            {
                return _gcodePoints;
            }
            set
            {
                Set(ref _gcodePoints, value);
            }
        }

        /// <summary>
        /// Get the Geometry property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PathGeometry PathGeometry
        {
            get
            {
                return _pathGeometry;
            }
            set
            {
                Set(ref _pathGeometry, value);
            }
        }
        /// <summary>
        /// Get the Fill property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush Fill
        {
            get
            {
                return _fill;
            }
            set
            {
                Set(ref _fill, value);
                logger.Info("MainViewModel|Image Fill: {0}", value);
            }
        }
        /// <summary>
        /// Get the Stroke property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush Stroke
        {
            get
            {
                return _stroke;
            }
            set
            {
                Set(ref _stroke, value);
                logger.Info("MainViewModel|Image Stroke: {0}", value);
            }
        }
        /// <summary>
        /// Get the StrokeThickness property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double StrokeThickness
        {
            get
            {
                return _strokeThickness;
            }
            set
            {
                Set(ref _strokeThickness, value);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Image StrokeThickness: {0}", value);

            }
        }
        /// <summary>
        /// Get the Paths property. 
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GraphicItems> GcodePaths
        {
            get
            {
                return _paths;
            }
            set
            {
                Set(ref _paths, value);
            }
        }
        #endregion
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
                logger.Info("***Program started***");
                _dataService = dataService;
                if (_dataService != null)
                {
                    _dataService.GetSerialPortSetting(
                        (item, error) =>
                        {
                            if (error != null)
                            {
                                logger.Error("MainViewModel|Exception GetSerialPortSetting raised: " + error);
                                return;
                            }
                            logger.Info("MainViewModel|Load serial port window");
                            _serialPort = new SerialPort();
                            GroupBoxPortSettingTitle = item.SerialPortSettingHeader;
                            GetSerialPortSettings();
                        });

                    _dataService.GetGCode(
                        (item, error) =>
                        {
                            if (error != null)
                            {
                                logger.Error("MainViewModel|Exception GetGCode raised: " + error);
                                return;
                            }
                            logger.Info("MainViewModel|Load GCode window");
                            GroupBoxGCodeFileTitle = item.GCodeHeader;
                            SelectedTransferDelay = ListTransferDelay[1];
                        });

                    _dataService.GetConsole(
                       (item, error) =>
                       {
                           if (error != null)
                           {
                               logger.Error("MainViewModel|Exception GetConsole raised: " + error);
                               return;
                           }
                           logger.Info("MainViewModel|Load Console window");
                           GroupBoxConsoleTitle = item.ConsoleHeader;
                       });

                    _dataService.GetGraphic(
                       (item, error) =>
                       {
                           if (error != null)
                           {
                               logger.Error("MainViewModel|Exception GetGraphic raised: " + error);
                               return;
                           }
                           logger.Info("MainViewModel|Load Graphic window");
                           GroupBoxGraphicTitle = item.GraphicHeader;
                       });

                    _dataService.GetJogging(
                       (item, error) =>
                       {
                           if (error != null)
                           {
                               logger.Error("MainViewModel|Exception GetJogging raised: " + error);
                               return;
                           }
                           logger.Info("MainViewModel|Load Jogging window");
                           GroupBoxJoggingTitle = item.JoggingHeader;
                       });
                }
                DefaultSettings();
                MyRelayCommands();
                MyMessengers();
                logger.Info("MainViewModel|MainWindow initialization finished");
        }
        #endregion

        #region Methods
        #region subregion relaycommands & messengers
        /// <summary>
        /// List of RelayCommands (MVVM) bind to button in ViewModels
        /// </summary>
        private void MyRelayCommands()
        {
            ConnectCommand = new RelayCommand(OpenSerialPort, CanExecuteOpenSerialPort);
            DisconnectCommand = new RelayCommand(CloseSerialPort, CanExecuteCloseSerialPort);
            RefreshPortCommand = new RelayCommand(RefreshSerialPort, CanExecuteRefreshSerialPort);
            SendManualCommand = new RelayCommand(SendData, CanExecuteSendData);
            SendM1Command = new RelayCommand(SendM1Data, CanExecuteSendM1Data);
            SendM2Command = new RelayCommand(SendM2Data, CanExecuteSendM2Data);
            SendM3Command = new RelayCommand(SendM3Data, CanExecuteSendM3Data);
            SendM4Command = new RelayCommand(SendM4Data, CanExecuteSendM4Data);
            ClearCommand = new RelayCommand(ClearData, CanExecuteClearData);
            GrblResetCommand = new RelayCommand(GrblReset, CanExecuteRealTimeCommand);
            GrblPauseCommand = new RelayCommand(GrblFeedHold, CanExecuteRealTimeCommand);
            GrblCurrentStatusCommand = new RelayCommand(GrblCurrentStatus, CanExecuteRealTimeCommand);
            GrblStartCommand = new RelayCommand(GrblStartCycle, CanExecuteRealTimeCommand);
            GrblRefreshSettingsCommand = new RelayCommand(GrblRefreshSettingAsync, CanExecuteOtherCommand);
            GrblParametersCommand = new RelayCommand(GrblParameters, CanExecuteOtherCommand);
            GrblParserStateCommand = new RelayCommand(GrblParserState, CanExecuteOtherCommand);
            GrblBuildInfoCommand = new RelayCommand(GrblBuildInfo, CanExecuteOtherCommand);
            GrblStartupBlocksCommand = new RelayCommand(GrblStartupBlocks, CanExecuteOtherCommand);
            GrblCheckCommand = new RelayCommand(GrblCheck, CanExecuteOtherCommand);
            GrblKillAlarmCommand = new RelayCommand(GrblKillAlarm, CanExecuteRealTimeCommand);
            GrblHomingCommand = new RelayCommand(GrblHoming, CanExecuteHomingCommand);
            GrblHome1Command = new RelayCommand(GrblHome1, CanExecuteHomingCommand);
            GrblSetHome1Command = new RelayCommand(GrblSetHome1, CanExecuteHomingCommand);
            GrblHome2Command = new RelayCommand(GrblHome2, CanExecuteHomingCommand);
            GrblSetHome2Command = new RelayCommand(GrblSetHome2, CanExecuteHomingCommand);
            GrblSleepCommand = new RelayCommand(GrblSleep, CanExecuteOtherCommand);
            GrblHelpCommand = new RelayCommand(GrblHelp, CanExecuteOtherCommand);
            JogHCommand = new RelayCommand<bool>(JogH, CanExecuteJog);
            JogNCommand = new RelayCommand<bool>(JogN, CanExecuteJog);
            JogSCommand = new RelayCommand<bool>(JogS, CanExecuteJog);
            JogECommand = new RelayCommand<bool>(JogE, CanExecuteJog);
            JogWCommand = new RelayCommand<bool>(JogW, CanExecuteJog);
            JogNWCommand = new RelayCommand<bool>(JogNW, CanExecuteJog);
            JogNECommand = new RelayCommand<bool>(JogNE, CanExecuteJog);
            JogSWCommand = new RelayCommand<bool>(JogSW, CanExecuteJog);
            JogSECommand = new RelayCommand<bool>(JogSE, CanExecuteJog);
            StepCommand = new RelayCommand<string>(GetStep,CanExecuteGetStep);
            IncreaseFeedRateCommand = new RelayCommand<bool>(IncreaseFeedRate, CanExecuteFeedRate);
            DecreaseFeedRateCommand = new RelayCommand<bool>(DecreaseFeedRate, CanExecuteFeedRate);
            ResetAxisXCommand = new RelayCommand(ResetAxisX, CanExecuteResetAxis);
            ResetAxisYCommand = new RelayCommand(ResetAxisY, CanExecuteResetAxis);
            StartLaserCommand = new RelayCommand(StartLaser, CanExecuteLaser);
            StopLaserCommand = new RelayCommand(StopLaser, CanExecuteLaser);
            LoadFileCommand = new RelayCommand(OpenFile, CanExecuteOpenFile);
            SendFileCommand = new RelayCommand(StartSendingFileAsync, CanExecuteStartAsyncTask);
            PauseFileCommand = new RelayCommand(PauseSendingFile, CanExecutePauseFile);
            StopFileCommand = new RelayCommand(StopSendingFileAsync, CanExecuteStopAsyncTask);
            IncreaseLaserPowerCommand = new RelayCommand<bool>(IncreaseLaserPower, CanExecuteLaserPower);
            DecreaseLaserPowerCommand = new RelayCommand<bool>(DecreaseLaserPower, CanExecuteLaserPower);
            logger.Info("MainViewModel|All RelayCommands loaded");
        }
        /// <summary>
        /// Used to communicate between ViewModels: SettingViewModel
        /// </summary>
        private void MyMessengers()
        {
            MessengerInstance.Register<NotificationMessage>(this, Test);
        }
        #endregion

        #region subregion serial port method
        /// <summary>
        /// Gets serial port settings from SerialPortSettingsModel class
        /// </summary>
        /// <param name="settingsInit"></param>
        public void GetSerialPortSettings()
        {
                _collectionPortName= new Collection<string>(SerialPort.GetPortNames());
                logger.Info("MainViewModel|Get serial port names");
        }
        /// <summary>
        /// Set default settings for serial port
        /// </summary>
        public void DefaultPortSetting()
        {
                SelectedBaudRate = 115200;
                IsBaudEnabled = true;
                IsPortEnabled = true;
                if (CollectionPortNames != null && CollectionPortNames.Count > 0)
                {
                    SelectedPortName = CollectionPortNames[0];
                }
                else
                {
                    logger.Info("MainViewModel|No port COM available");
                }
                logger.Info("MainViewModel|Serial port default settings loaded");
        }
        /// <summary>
        /// Reload serial port settings and set default values.
        /// </summary>
        public void RefreshSerialPort()
        {
            logger.Info("MainViewModel|Refresh serial port");
            GetSerialPortSettings();
            DefaultPortSetting();
        }
        /// <summary>
/// Allows/Disallows RefreshSerialPort method to be executed.
/// </summary>
/// <returns></returns>
        public bool CanExecuteRefreshSerialPort()
        {
                return !_serialPort.IsOpen;
        }
        /// <summary>
        /// Starts serial port communication
        /// </summary>
        public void OpenSerialPort()
        {
            try
            {
                _serialPort.PortName = SelectedPortName;
                _serialPort.BaudRate = SelectedBaudRate;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.ReadBufferSize = 100;
                _serialPort.WriteBufferSize = 100;
                _serialPort.ReceivedBytesThreshold = 10;
                _serialPort.DiscardNull = false;
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();
                logger.Info("MainViewModel|Port COM open");
                IsBaudEnabled = false;
                IsPortEnabled = false;
                StartCommunication();
            }
            catch (IOException ex)
            {
                logger.Error("MainViewModel|Exception OpenSerialPort raised: " + ex.ToString());
            }
        }
        /// <summary>
        /// Allow/Disallow OpenSerialPort method to be executed.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteOpenSerialPort()
        {
            return !_serialPort.IsOpen && !String.IsNullOrEmpty(SelectedPortName);
        }
        /// <summary>
        /// Start timer and infos
        /// </summary>
        public void StartCommunication()
        {
            if(_serialPort.IsOpen)
            {
                GrblReset(); //Do a soft reset before starting a new job?
                GrblBuildInfo();
                //GrblRefreshSettingAsync();
                CheckInfos();
                if (!currentStatusTimer.IsEnabled)
                {
                    InitializeDispatcherTimer();
                }
            }
            else
            {
                Cleanup();
                CloseSerialPort();
                logger.Info("MainViewModel|Wrong com port", VersionGrbl);
            }
        }
        /// <summary>
    /// End serial port communication
    /// </summary>
        public void CloseSerialPort()
        {
            try
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.Dispose();
                _serialPort.Close();
                IsBaudEnabled = true;//In cleanup?
                IsPortEnabled = true;
            }
            catch(InvalidOperationException ex)
            {
                logger.Error(ex.GetType().FullName);
                logger.Error(ex.Message);
                logger.Error("MainViewModel|Exception CloseSerialPort raised: " + ex.ToString());
            }
            finally
            {
                Cleanup();
                logger.Info("MainViewModel|Port COM closed");
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
        /// Check grbl version 0.9 or 1.1.
        /// </summary>
        public void CheckInfos()
        {
                bool checkVersion = false;
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Checking Grbl version");
                while (!checkVersion||VersionGrbl=="-")
                {
                Thread.Sleep(50);

                if (VersionGrbl.StartsWith("0") || VersionGrbl.StartsWith("1"))//Does not work neither readline in GrblBuildInfo to check grbl version...
                    {
                    logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Grbl version {0}", VersionGrbl);
                    checkVersion = true;
                    }
                    else
                    {
                    logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Bad Grbl version {0}", VersionGrbl);
                    checkVersion = true;
                    }
                }
        }
        /// <summary>
        /// Sends G-code or Grb data (TXLine in manual Send data group box) to serial port.
        /// </summary>
        public void SendData()
        {
                WriteString(TXLine);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Manual Data TX: {0}", TXLine);
                TXLine = string.Empty;
        }
        /// <summary>
        /// Allow/Disallow the Senddata method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteSendData()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(TXLine))
            {
                if (ResponseStatus == RespStatus.Ok)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Sends Macro1 to serial port.
        /// </summary>
        public void SendM1Data()
        {
                //_serialPort.WriteLine(Macro1);
            WriteString(Macro1);
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Macro1 Data TX: {0}", Macro1);
        }
        public bool CanExecuteSendM1Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro1) && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
            {
                //if (PlannerBuffer == "0" && ResponseStatus == RespStatus.Ok)
                if (ResponseStatus == RespStatus.Ok)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Sends Macro2 to serial port.
        /// </summary>
        public void SendM2Data()
        {
            //_serialPort.WriteLine(Macro2);
            WriteString(Macro2);

            logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Macro2 Data TX: {0}", Macro2);
        }
        public bool CanExecuteSendM2Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro2) && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
                {
                if (ResponseStatus == RespStatus.Ok)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Sends Macro3 to serial port.
        /// </summary>
        public void SendM3Data()
        {
            //_serialPort.WriteLine(Macro3);
            WriteString(Macro3);
            logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Macro3 Data TX: {0}", Macro3);
        }
        public bool CanExecuteSendM3Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro3) && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
            {
                if (ResponseStatus == RespStatus.Ok)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Sends Macro4 to serial port.
        /// </summary>
        public void SendM4Data()
        {
            //_serialPort.WriteLine(Macro4);
            WriteString(Macro4);
            logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Macro4 Data TX: {0}", Macro4);
        }
        public bool CanExecuteSendM4Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro4) && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
            {
                if (ResponseStatus == RespStatus.Ok)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Writes byte (7 bits) to serial port.
        /// </summary>
        /// <param name="b"></param>
        public void WriteByte(byte b)
        {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Write(new byte[1] { b }, 0, 1);
                    if (b != 63)//Skips current status logger with DispatcherTimer
                    {
                        logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Method WriteByte: {0}", b);
                    }
                }
        }
        /// <summary>
        /// Writes bytes to serial port.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="lengh"></param>
        public void WriteBytes(byte[] buffer, int lengh)
        {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Write(buffer, 0, lengh);
                    logger.Info("MainViewModel|Method WriteBytes: {0}", buffer.ToString());
                }
        }
        /// <summary>
        /// Writes a string to serial port
        /// </summary>
        /// <param name="data"></param>
        public void WriteString(string data)
        {
                if (_serialPort.IsOpen)
                {
                    _serialPort.WriteLine(data);
                    TXLine = data;
                }
                logger.Info("MainViewModel|Method WriteString: {0} done {1}", data, _serialPort.IsOpen);
        }
        /// <summary>
        /// Clears group box Send and Data received + serial port and Grbl buffers
        /// </summary>
        public void ClearData()
        {
            logger.Info("MainViewModel|Clear data");
                if (_serialPort.IsOpen)
                {
                    GrblReset();
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    ListConsoleData.Clear();
                    ListGrblSettings.Clear();
                    SettingCollection.Clear();
                    logger.Info("MainViewModel|TXLine/RXLine and buffers erased");
                }
                Thread.Sleep(50);
                
                if (FileQueue.Count > 0)
                {
                    FileQueue.Clear();
                    
                }
            if (GCodeData != null)
            {
                GCodeData.Clear();
            }
            if (GcodePaths != null)
            {
                GcodePaths.Clear();
            }
            FileName = string.Empty;
            RXLine = string.Empty;
            TXLine = string.Empty;
            GCodeLine = string.Empty;
            NLine = 0;
                PercentLine = 0;
                RLine = 0;
                EstimateJobTime = "00:00:00" ;
        }
        /// <summary>
        /// Allow/Disallow the cleardata method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteClearData()
        {
            if (_serialPort.IsOpen)
            {
                return IsManualSending && Buf == "15";
            }
            else
            {
                return true;
                    }
        }
        #endregion

        #region subregion Grbl commands
        /// <summary>
        /// Writes Grbl real-time command (asci dec 24) or 0x18 (Ctrl-x) for a soft reset
        /// </summary>
        public void GrblReset()
        {
            WriteByte(24);
            logger.Info("MainViewModel|Soft reset");
        }
       
        /// <summary>
        /// Writes Grbl "~" real-time command (ascii dec 126) to start or resume the machine after a pause or 'M0' command.
        /// </summary>
        public void GrblStartCycle()
        {
            WriteByte(126);
            logger.Info("MainViewModel|Start/Resume");
        }
        
        /// <summary>
        /// Writes Grbl "!" real-time command (ascii dec 33) to pause the machine motion X, Y and Z (not spindle or laser).
        /// </summary>
        public void GrblFeedHold()
        {
            WriteByte(33);
            logger.Info("MainViewModel|Pause");
        }

        /// <summary>
        /// Writes Grbl "?" real-time command "?" (Ascii dec 63) to get immadiate status report of the machine.
        /// </summary>
        public void GrblCurrentStatus()
        {
            WriteByte(63);
        }

        /// <summary>
        /// Write Grbl '$X' command to kill alarm mode. In real-time command and canexecuterealTimeCommand in order to kill alarm
        /// </summary>
        public void GrblKillAlarm()
        {
            WriteString("$X");
            logger.Info("MainViewModel|Kill alarm mode");
        }

        /// <summary>
        /// Allows/disallows real-time command. These commands can be sent at anytime,
        /// anywhere, and Grbl will immediately respond, no matter what it's doing
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteRealTimeCommand()
        {
            return _serialPort.IsOpen;
        }

        /// <summary>
        /// Sends in async mode the Grbl '$$' command to get all particular $x=var settings of the machine
        /// </summary>
        private async void GrblRefreshSettingAsync()
        {
            logger.Info("MainViewModel|Load Grbl settings");
            if (ListGrblSettings.Count > 0)
            {
                ListGrblSettings.Clear();
            }
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Task task = Task.Run(() =>
            {
               WriteString("$$");
               manualResetEvent.WaitOne();
               SettingCollection = new ObservableCollection<SettingItem>(ListGrblSettings);
               if (!IsRefresh)
               {
                   cts.Cancel();
               }
               if (token.IsCancellationRequested)
               {
                   logger.Info("MainViewModel|Task load settings canceled");
                   //throw new TaskCanceledException(t);
               }
           },token);
            try
            {
                //await task;
                await task.ConfigureAwait(false);
                logger.Info("MainViewModel|Task load settings completed");
            }

            catch (OperationCanceledException ex)
            {
                logger.Info("MainViewModel|Task load settings cancelled"+ex.ToString());
            }

            finally
            {
                cts.Dispose();
                logger.Info("MainViewModel|Task load settings cleared");
            }
        }
        
        /// <summary>
        /// Writes Grbl '$#' command to view parameters [G54:0.000,0.000,0.000].
        /// </summary>
        public void GrblParameters()
        {
            WriteString("$#");
            logger.Info("MainViewModel|Grbl parameter");
        }

        /// <summary>
        /// Writes Grbl '$G' command to view G-code parser state [G0 G54 G17 G21 G90 G94 M0 M5 M9 T0 F0. S0.].
        /// </summary>
        public void GrblParserState()
        {
            WriteString("$G");
            logger.Info("MainViewModel|Grbl parser mode");
        }

        /// <summary>
        /// Writes Grbl '$I' command to view version and date of build [0.9i.20150620:].
        /// </summary>
        public void GrblBuildInfo()
        {
            WriteString("$I");
            logger.Info("MainViewModel|Grbl infos");
        }

        /// <summary>
        /// Write Grbl '$N' command to view startup blocks $N0=.
        /// </summary>
        public void GrblStartupBlocks()
        {
            WriteString("$N");
            logger.Info("MainViewModel|Grbl startup block");
        }

        /// <summary>
        /// Write Grbl '$C' command to enable check mode (No motion).
        /// </summary>
        public void GrblCheck()
        {
            WriteString("$C");
            logger.Info("MainViewModel|Grbl check mode");
        }

        /// <summary>
        /// Write Grbl '$SLP' command to enable sleep mode
        /// </summary>
        public void GrblSleep()
        {
            WriteString("$SLP");
            logger.Info("MainViewModel|Grbl sleep mode");
        }

        /// <summary>
        /// Write Grbl '$' command to get help.
        /// </summary>
        public void GrblHelp()
        {
            WriteString("$");
            logger.Info("MainViewModel|Grbl help");
        }

        /// <summary>
        /// Allows/disallows Grbl's Other '$' Commands. The other $ commands provide additional controls for the user, 
        /// such as printing feedback on the current G-code parser modal state or running the homing cycle.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteOtherCommand()
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
            {
                if (ResponseStatus != RespStatus.NOk)
                {
                    return true;
                }
            }
            return false;
            //return true;
        }

        /// <summary>
        /// Write Grbl '$H' command to run homing cycle.
        /// </summary>
        public void GrblHoming()
        {
            if (MachineStatus == MachStatus.Alarm)//At startup, when homing is activated, machin is in alarm mode
            {
                GrblReset();
                GrblKillAlarm();
                Thread.Sleep(100);
            }
            if (IsLaserPower)//By default during homing or G0 mode the laser is deactivated but still SXXX value. For safety I put it off.
            {
                StopLaser();
            }
            WriteString("$H");
            MachineStatus = MachStatus.Home;
            MachineStatusColor = Brushes.LightPink;
            logger.Info("MainViewModel|Grbl homing");
        }

        /// <summary>
        /// Write Grbl 'G28' command to go to the pre-defined position 1 set by G28.1 command.
        /// </summary>
        public void GrblHome1()
        {

            WriteString("G28");
            logger.Info("MainViewModel|Grbl home 1");
        }

        /// <summary>
        /// Write Grbl 'G28.1' command to set the pre-defined position 1.
        /// </summary>
        public void GrblSetHome1()
        {

            WriteString("G28.1");
            logger.Info("MainViewModel|Grbl set home 1");
        }

        /// <summary>
        /// Write Grbl 'G30' command to go to the pre-defined position 1 set by G30.1 command.
        /// </summary>
        public void GrblHome2()
        {

            WriteString("G30");
            logger.Info("MainViewModel|Grbl home 2");
        }

        /// <summary>
        /// Write Grbl 'G30.1' command to set the pre-defined position 2.
        /// </summary>
        public void GrblSetHome2()
        {
            WriteString("G30.1");
            logger.Info("MainViewModel|Grbl set home 2");
        }

        /// <summary>
        /// Allows/disallows Grbl's Other '$' Commands. The other $ commands provide additional controls for the user, 
        /// such as printing feedback on the current G-code parser modal state or running the homing cycle.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteHomingCommand()
        {
            if (_serialPort.IsOpen)
            {
                    return true;
            }
            return false;
        }
        #endregion

        #region subregion G-code commands
        /// <summary>
        /// Rapid move to home position.
        /// </summary>
        public void JogH(bool parameter)
        {
            string line = GCodeTool.FormatGcode(0,0, 0, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogH: {0}", line);
        }

        /// <summary>
        /// Move one step Y+
        /// </summary>
        public void JogN(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, 0, -1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogN: {0}", line);
        }

        /// <summary>
        /// Move one step Y-
        /// </summary>
        public void JogS(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, 0, 1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogS: {0}", line);
        }

        /// <summary>
        /// Move one step X+
        /// </summary>
        public void JogE(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, 1, 0, FeedRate, Double.Parse(Step.Replace('.',',')));
            WriteString(line);
            logger.Info("MainViewModel|JogE: {0}", line);
        }

        /// <summary>
        /// Move one step X-
        /// </summary>
        public void JogW(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, -1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogW: {0}", line);
        }

        /// <summary>
        /// Move one step X- Y+
        /// </summary>
        public void JogNW(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, -1, -1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogNW: {0}", line);
        }

        /// <summary>
        /// Move one step X+ Y+
        /// </summary>
        public void JogNE(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, 1, -1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogNE: {0}", line);
        }

        /// <summary>
        /// Move one step X- Y-
        /// </summary>
        public void JogSW(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, -1, 1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogSW: {0}", line);
        }

        /// <summary>
        /// Move one step X+ Y-
        /// </summary>
        public void JogSE(bool parameter)
        {
            string line = GCodeTool.FormatGcode(1,1, 1, 1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogSE: {0}", line);
        }

        /// <summary>
        /// Allows jogging mode if keyboard is not checked.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteJog(bool parameter)
        {
            //if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && VersionGrbl.StartsWith("0"))
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus!=MachStatus.Home && MachineStatus!=MachStatus.Alarm)
            {
                if (parameter)
                {
                    return true;
                }
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
                if (!string.IsNullOrEmpty(parameter)&&_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
                {
                    return true;
                }
            return false;
        }
        
        //TODO: add set zero position in joggingview G92 X0 Y0...

        /// <summary>
        /// Increase the motion speed with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        private void IncreaseFeedRate(bool parameter)
        {
            FeedRate += 10;
            logger.Info("MainViewModel|F{0}", _manualfeedRate);
        }

        /// <summary>
        /// Decrease the motion speed with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        private void DecreaseFeedRate(bool parameter)
        {
            FeedRate -= 10;
            logger.Info("MainViewModel|F{0}", _manualfeedRate);
        }

        /// <summary>
        /// Allows/Disallows feed rate motion increase/decrease methods.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CanExecuteFeedRate(bool parameter)
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
            {
                if (parameter)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Start laser mode
        /// </summary>
        private void StartLaser()
        {
            WriteString("M3");
            LaserPower = 10;
            Speed = LaserPower.ToString();
            WriteString(string.Format("S{0}", _laserPower));
            LaserColor = Brushes.Blue;
            IsLaserPower = true;
        }

        /// <summary>
        /// Stop laser mode
        /// </summary>
        private void StopLaser()
        {
            LaserPower = 0;
            Speed = LaserPower.ToString();
            WriteString(string.Format("S{0}", _laserPower));
            LaserColor = Brushes.LightGray;
            IsLaserPower = false;
        }

        /// <summary>
        /// Allows/Disallows Laser methods.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteLaser()
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
                return true;
            else
                return false;
            ;
        }

        /// <summary>
        /// Increase the laser power with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        private void IncreaseLaserPower(bool parameter)
        {
            LaserPower += 10;
            logger.Info("MainViewModel|S{0}", _laserPower);
        }

        /// <summary>
        /// Decrease the laser power with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        private void DecreaseLaserPower(bool parameter)
        {
            LaserPower -= 10;
            logger.Info("MainViewModel|S{0}", _laserPower);
        }

        /// <summary>
        /// Allows/Disallows feed rate motion increase/decrease methods.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CanExecuteLaserPower(bool parameter)
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
            {
                if (parameter)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets current axis X to 0.
        /// </summary>
        public void ResetAxisX()
        {
            //string line = "G10 P0 L20 X0";
            //string line = "G10 L2 P1 X0";
            //string line = "G10 P0 L2 X0";
            string line = "G92 X0";
            WriteString(line);
            logger.Info("MainViewModel|Reset X: {0}", line);
        }

        /// <summary>
        /// Sets current axis Y to 0.
        /// </summary>
        public void ResetAxisY()
        {
            //string line = "G10 P0 L20 Y0";
            string line = "G92 X0";
            WriteString(line);
            logger.Info("MainViewModel|Reset Y: {0}", line);
        }

        /// <summary>
        /// Allows/Disallows Reset axis. G10 command only available w/ 0.9j and above?
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteResetAxis()
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
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
            else return false;
        }
        #endregion

        #region subregion other methods
        /// <summary>
        /// Set default settings
        /// </summary>
        public void DefaultSettings()
        {
            DefaultPortSetting();
            DefaultLaserSetting();
            DefaultGraphicSetting();
        }

        /// <summary>
        /// Set default laser settings
        /// </summary>
        public void DefaultLaserSetting()
        {
            LaserPower = 0;
            LaserColor = Brushes.LightGray;
            SelectedLaser = 2500;
            MaxLaserPower = SelectedLaser;
            ConverterParameterLaser = 25;
            logger.Info("MainViewModel|Default laser settings completed");
        }

        /// <summary>
        /// Set default graphic settings
        /// </summary>
        public void DefaultGraphicSetting()
        {
            GraphicTool gt = new GraphicTool();
            GcodePaths.Add(new GraphicItems
            {
                GraphicPathGeometry = gt.Axis(160, 160, 0),
                GraphicFill = Fill,
                GraphicStroke = Stroke,
                GraphicStrokeThickness = StrokeThickness,
            });
            logger.Info("MainViewModel|Default Coordinate Plane");
        }

        /// <summary>
        /// Does nothing yet...
        /// </summary>
        public override void Cleanup()
        {
            if (currentStatusTimer.IsEnabled)
            {
                currentStatusTimer.Stop();
            }
            if (IsLaserPower)
            {
                StopLaser();
            }
            base.Cleanup();
            logger.Info("MainViewModel|Clean...");
        }

        /// <summary>
        /// Initializes DispatcherTimer to query Grbl report state at 4Hz (5Hz is max recommended).
        /// </summary>
        private void InitializeDispatcherTimer()
        {
            currentStatusTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            currentStatusTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            currentStatusTimer.Start();
            logger.Info("MainViewModel|Initialize Dispatcher Timer");
        }

        /// <summary>
        /// Selects the G-code file to send to the Arduino.
        /// </summary>
        public void OpenFile()
        {
            if (FileQueue!=null)
            {
                ClearData();
            }
            OpenFileDialog openFile = new OpenFileDialog();
                logger.Info("MainViewModel|OpenFile");
                openFile.Title = "Fichier G-code";
                openFile.Filter = "G-Code files|*.txt;*.gcode;*.ngc;*.nc,*.cnc|Tous les fichiers|*.*";
                openFile.FilterIndex = 1;
                openFile.DefaultExt = ".txt";
                if(openFile.ShowDialog().Value)
                {
                    FileName = openFile.FileName;
                    LoadFile(FileName);
                }
        }

        /// <summary>
        /// Allows/Disallows LoadFile method.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteOpenFile()
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm)
            {
                return true;
            }
            //else return false;
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Load G-Code file and save lines to a queue and a list.
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadFile(string fileName)
        {
            string line;
            int i = 0;
            FileQueue.Clear();
            FileList.Clear();
            GCodeData = new ObservableCollection<GCodeModel>();
            using (StreamReader sr = new StreamReader(fileName))
            {
                while((line=sr.ReadLine())!=null)
                {
                    //FileQueue.Enqueue(gcodeToolBasic.TrimGcode(line).Replace(',', '.'));
                    FileQueue.Enqueue(line.Replace(',', '.'));
                    FileList.Add(line.Replace('.', ','));
                    GCodeModel = new GCodeModel(i,line);
                    GCodeData.Add(GCodeModel);
                    i++;
                }
            }
            NLine = FileQueue.Count;
            RLine = NLine;
            logger.Info("MainViewModel|Get GCode FileList");
            gcodeTool = new GCodeTool(FileList);
            logger.Info("MainViewModel|Get GCode PointCollection");
            //GCodePoints = gcodeTool.GetGCodePointCollection(50,50);
            GCodePoints = gcodeTool.GetGCodePointCollection(10, 10, 0.5);
            TimeSpan time = TimeSpan.FromSeconds(Math.Round(gcodeTool.CalculateJobTime(MaxFeedRate)));
            EstimateJobTime = time.ToString(@"hh\:mm\:ss");
            GCodeDrawing(GCodePoints);
        }

        /// <summary>
        /// Send the G-Code file in async mode.
        /// </summary>
        private async void StartSendingFileAsync()
        {
            if (MachineStatus == MachStatus.Hold)
            {
                GrblStartCycle();
                ResponseStatus = RespStatus.Ok;
            }
            else
            {
                cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                var progressHandler = new Progress<double>(value => PercentLine = value);
                var progress = progressHandler as IProgress<double>;
                //var task = Task.Run(() =>
                Task task = Task.Run(() =>
                {
                    for (int i = 0; i <= NLine; i++)
                    {
                        SendFile();
                        logger.Info("MainViewModel|N{0}: {1}", i.ToString(CultureInfo.CurrentCulture), TXLine);
                        if (progress != null && NLine != 0)
                        {
                            progress.Report((NLine - RLine) / NLine);
                        }
                        else
                        {
                            progress.Report(0);
                        }
                        manualResetEvent.WaitOne();//Wait for the signal ok to continue task...
                        //logger.Info("MainViewModel|mre waitone");
                        Thread.Sleep(transferDelay);
                        if (!IsSending)
                        {
                            cts.Cancel();
                        }
                        if (token.IsCancellationRequested)
                        {
                            logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Sending file canceled at line {0}", i.ToString());
                            //throw new TaskCanceledException(t);
                            break;
                        }
                    }
                }, token);

                transferDelay = SelectedTransferDelay switch
                {
                    "Slow" => 2000,
                    "Normal" => 750,
                    "Fast" => 250,
                    "UltraFast" => 0,
                    _ => 750,
                };
                try
                {
                    await task.ConfigureAwait(false);
                    logger.Info("MainViewModel|Task sending file completed");
                }

                catch (OperationCanceledException ex)
                {
                    logger.Info("MainViewModel|Task sending file cancelled" + ex.ToString());
                }

                finally
                {
                    cts.Dispose();
                    logger.Info("MainViewModel|Task sending file cleared");
                }
            }
        }
        /// <summary>
        /// Cancel the task to send G-code file in async mode. Pause the machine and clear queue. TOBEIMPROVE
        /// Used with stop button.
        /// </summary>
        private void StopSendingFileAsync()
        {
            if (cts != null)
            {
                logger.Info("MainViewModel|Stop sending file");
                try
                {
                    cts.Cancel();
                }
                catch (ObjectDisposedException ex)
                {
                    logger.Error("MainViewmodel|StopSendingFileAsync " + ex.ToString());
                }
            }
                GrblFeedHold();
                ResponseStatus = RespStatus.Q;
                NLine = FileQueue.Count;
                FileQueue.Clear();
                IsSending = false;
                IsManualSending = true;
        }
        /// <summary>
        /// Allows/Disallows StartSendFileAsynk method.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteStartAsyncTask()
        {
            if (FileQueue.Count > 0 && _serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm && MachineStatus != MachStatus.Run)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Allows/Disallows StopSendingFileAsynk method.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteStopAsyncTask()
        {
            if (FileQueue.Count > 0 && _serialPort.IsOpen && ResponseStatus != RespStatus.NOk)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Pause the machine and clear queue. TODO Use "mre" to pause task and resume...
        /// Used with stop button.
        /// </summary>
        private void PauseSendingFile()
        {
                logger.Info("MainViewModel|Pause sending file");
                GrblFeedHold();
                ResponseStatus = RespStatus.Q;
                //IsSending = false;
        }

        /// <summary>
        /// Allows/Disallows SendFile method.
        /// </summary>
        /// <returns></returns>
        public bool CanExecutePauseFile()
        {
            if (FileQueue.Count > 0 && _serialPort.IsOpen && ResponseStatus != RespStatus.NOk && MachineStatus != MachStatus.Home && MachineStatus != MachStatus.Alarm && MachineStatus != MachStatus.Hold)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sends G-code file line by line =>startSendingFile
        /// </summary>
        public void SendFile()
        {
            RLine = FileQueue.Count;
            string line;
            if (RLine > 0 && (int)ResponseStatus != 1)
            {
                if (MachineStatus != MachStatus.Alarm)
                {
                    ResponseStatus = RespStatus.Q;
                    line = FileQueue.Dequeue();
                    WriteString(line);
                    IsManualSending = false;
                    IsSending = true;
                    //logger.Info(CultureInfo.CurrentCulture, "MainViewModel|BytesToWrite : {0}", _serialPort.BytesToWrite);
                }
                else
                {
                    IsSending = false;
                    IsManualSending = true;
                }
            }
            else
            {
                IsManualSending = true;
                IsSending = false;
            }
        }
        #endregion

        #region subregion graphic
        /// <summary>
        /// Draw G-code file with a path.
        /// </summary>
        public void GCodeDrawing(PointCollection pc)
        {
            GraphicTool graphicTool = new GraphicTool(pc);

            logger.Info("MainViewModel|GrblTest Geometry");

            GcodePaths.Add(new GraphicItems
            {
                GraphicPathGeometry = graphicTool.Plotter(),
                GraphicFill = Fill,
                GraphicStroke = Brushes.Red,
                GraphicStrokeThickness = StrokeThickness,
            });
        }
        #endregion
        
        /// <summary>
        /// Write a notification message to serial port
        /// This is a test command bind to TEST button in SettingViewModel for development purpose only.
        /// </summary>
        /// <param name="notificationMessage"></param>
        public void Test(NotificationMessage notificationMessage)
        {
            string data = notificationMessage.Notification;
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.WriteLine(data);
                    TXLine = data;
                }
                logger.Info(CultureInfo.CurrentCulture, "MainViewModel|Method Test: {0}", data);
            }
            catch (IOException ex)
            {
                logger.Error("MainViewModel|Exception Test raised: " + ex.ToString());
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// Get all data from serial port and process response.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
                string line = _serialPort.ReadLine();
                RXLine = GCodeTool.TrimEndGcode(line);
                Console = new ConsoleModel(TXLine, RXLine);
                if (IsVerbose)
                {
                    ListConsoleData.Add(Console);
                }
                else if (!line.StartsWith("<",StringComparison.OrdinalIgnoreCase)&&!line.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                { 
                    ListConsoleData.Add(Console);
                }
                grbltool.DataGrblSorter(line);
                ResponseStatus = (RespStatus)grbltool.ResponseStatus;
            try
            {
                int b = Convert.ToInt32(Buf, CultureInfo.CurrentCulture);
                int r = Convert.ToInt32(RX, CultureInfo.CurrentCulture);

                if (ResponseStatus==RespStatus.Ok && r>0)//Need to have buffer available $10=3
                {
                    manualResetEvent.Set();
                    //logger.Info("mre set");
                    manualResetEvent.Reset();
                    //logger.Info("mre reset");
                }
            }
            catch (FormatException)
            {
            }
            MachineStatus = (MachStatus)grbltool.MachineStatus;
                MachineStatusColor = (SolidColorBrush)grbltool.MachineStatusColor;
                MPosX = grbltool.MachinePositionX;
                MPosY = grbltool.MachinePositionY;
                WPosX = grbltool.WorkPositionX;
                WPosY = grbltool.WorkPositionY;
                Feed = grbltool.MachineFeed;
                Speed = grbltool.MachineSpeed;
                if(Speed!="0"||String.IsNullOrEmpty(Speed))
                {
                    LaserColor = Brushes.Blue;
                }
                else
                {
                    LaserColor = Brushes.LightGray;
                }
                Buf = grbltool.PlannerBuffer;
                RX = grbltool.RxBuffer;
                VersionGrbl = grbltool.VersionGrbl;
                BuildInfoGrbl = grbltool.BuildInfo;
                InfoMessage = grbltool.InfoMessage;
                ConsoleData = new ObservableCollection<ConsoleModel>(ListConsoleData);
                if (ListConsoleData.Count>5)
                {
                    ListConsoleData.RemoveAt(0);
                }
            ListGrblSettings = grbltool.ListGrblSettings;
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
        /// Implement the method for IDispose interface
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
                    cts.Dispose();
                    manualResetEvent.Dispose();
                    logger.Info("MainViewModel|Disposed");
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