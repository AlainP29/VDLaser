using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VDGrbl.Model;
using VDGrbl.Tools;

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

        private int _selectedBaudRate;
        private int transferDelay;

        private string _selectedPortName = string.Empty;
        private string _versionGrbl = "-", _buildInfoGrbl = "-";
        private string _mposX = "0.000", _mposY = "0.000";
        private string _wposX = "0.000", _wposY = "0.000";
        private string _step = "1";
        private string _buf = "0", _rx = "0";
        private string _errorMessage = string.Empty;
        private string _alarmMessage = string.Empty;
        private string _infoMessage = string.Empty;
        private string _fileName = string.Empty;
        private string _imagePath = string.Empty;
        private string _estimateJobTime = "00:00:00";
        private string _groupBoxPortSettingTitle = string.Empty;
        private string _groupBoxGrblSettingTitle = string.Empty;
        private string _groupBoxGrblConsoleTitle = string.Empty;
        private string _groupBoxGrblCommandTitle = string.Empty;
        private string _groupBoxGCodeFileTitle = string.Empty;
        private string _groupBoxCoordinateTitle = string.Empty;
        private string _groupBoxImageTitle = string.Empty;
        private string _selectedTransferDelay = string.Empty;

        private string _txLine = string.Empty;
        private string _rxLine = string.Empty;
        private string _gcodeLine = string.Empty;
        private string _macro1 = "G90 G0 X0", _macro2 = "G91 G1 X10 Y-20", _macro3 = "G90 G0 Y0 F2000", _macro4 = "G91 G1 X-20 Y10 F1000";

        private double _feedRate = 300;
        private double _nLine = 0;
        private double _rLine = 0;
        private double _percentLine = 0;
        private double _laserPower = 0;

        private bool _isSelectedKeyboard=false;
        private bool _isSelectedMetric = true;
        private bool _isJogEnabled=true;
        private bool _isVerbose = false;
        private bool _isManualSending = true;
        private bool _isSending = false;

        //private BitmapSource _imgSource = null;
        private BitmapSource _imgTransform=null;

        private SerialPort _serialPort;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        DispatcherTimer currentStatusTimer = new DispatcherTimer(DispatcherPriority.Normal);
        private CancellationTokenSource cts;
        private RespStatus _responseStatus = RespStatus.Ok;
        private MachStatus _machineStatus = MachStatus.Idle;
        private SolidColorBrush _machineStatusColor = new SolidColorBrush(Colors.LightGray);
        private SolidColorBrush _laserColor = new SolidColorBrush(Colors.LightGray);
        private ObservableCollection<GrblModel> _settingCollection;
        private List<GrblModel> _listGrblSettingModel = new List<GrblModel>();
        private Queue<string> _fileQueue = new Queue<string>();
        private List<string> _fileList = new List<string>();
        private ObservableCollection<GrblModel> _consoleData;
        private List<GrblModel> _listConsoleData = new List<GrblModel>();
        private GrblModel _grblModel=new GrblModel("TX","RX");
        private GrblTool grbltool = new GrblTool();
        private Tools.GCodeTool gcodeToolBasic = new Tools.GCodeTool();
        #region subregion enum
        /// <summary>
        /// Enumeration of the response states. Ok: All is good, NOk: Alarm state Q: Queued [DR: Data received] 
        /// </summary>
        public enum RespStatus { Ok, NOk, Q };

        /// <summary>
        /// Enumeration of the machine states.
        /// </summary>
        public enum MachStatus { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep };

        /// <summary>
        /// Enumeration of the speed state : delay between two G-code lines.
        /// </summary>
        public enum SendSpeedStatus { Slow, Normal, Fast};
        #endregion
        #endregion

        #region public Properties
        #region subregion Relaycommands
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        public RelayCommand RefreshPortCommand { get; private set; }
        public RelayCommand SendCommand { get; private set; }
        public RelayCommand SendM1Command { get; private set; }
        public RelayCommand SendM2Command { get; private set; }
        public RelayCommand SendM3Command { get; private set; }
        public RelayCommand SendM4Command { get; private set; }
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
        public RelayCommand<bool> DecreaseFeedRateCommand { get; private set; }
        public RelayCommand<bool> IncreaseFeedRateCommand { get; private set; }
        public RelayCommand ResetAxisXCommand { get; private set; }
        public RelayCommand ResetAxisYCommand { get; private set; }
        public RelayCommand ResetAxisZCommand { get; private set; }
        public RelayCommand ResetAllAxisCommand { get; private set; }
        public RelayCommand StartLaserCommand { get; private set; }
        public RelayCommand StopLaserCommand { get; private set; }
        public RelayCommand LoadFileCommand { get; private set; }
        public RelayCommand LoadImageCommand { get; private set; }
        public RelayCommand SendFileCommand { get; private set; }
        public RelayCommand StopFileCommand { get; private set; }
        public RelayCommand<bool> DecreaseLaserPowerCommand { get; private set; }
        public RelayCommand<bool> IncreaseLaserPowerCommand { get; private set; }
        #endregion

        #region subregion setting
        /// <summary>
        /// Get the GroupBoxGrblSettingTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxGrblSettingTitle
        {
            get
            {
                return _groupBoxGrblSettingTitle;
            }
            set
            {
                Set(ref _groupBoxGrblSettingTitle, value);
            }
        }

        /// <summary>
        /// Gets the ListSettingModel property. ListSettingsModel is populated w/ Grbl settings data ('$$' command)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<GrblModel> ListGrblSetting
        {
            get
            {
                return _listGrblSettingModel;
            }
            set
            {
                Set(ref _listGrblSettingModel, value);
            }
        }

        /// <summary>
        /// Gets the SettingCollection property. SettingCollection is populated w/ data from ListSettingModel
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GrblModel> SettingCollection
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
        public List<GrblModel> ListConsoleData
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
        public ObservableCollection<GrblModel> ConsoleData
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
            }
        }
        #endregion

        #region subregion send
        /// <summary>
        /// Get the GroupBoxGrblCommandTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxGrblCommandTitle
        {
            get
            {
                return _groupBoxGrblCommandTitle;
            }
            set
            {
                Set(ref _groupBoxGrblCommandTitle, value);
            }
        }

        /// <summary>
        /// Get the GrblModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GrblModel GrblModel
        {
            get
            {
                return _grblModel;
            }
            set
            {
                Set(ref _grblModel, value);
            }
        }
        
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
        /// Gets the TXLine property. TXLine is the transmetted G-Code or Grbl commands to the Arduino
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
                logger.Info("MainViewModel|Macro 1: {0}", value);
            }
        }
      
        /// <summary>
        /// Gets the TXLine property. TXLine is the transmetted G-Code or Grbl commands to the Arduino
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
                logger.Info("MainViewModel|Macro 2: {0}", value);
            }
        }

        /// <summary>
        /// Gets the TXLine property. TXLine is the transmetted G-Code or Grbl commands to the Arduino
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
                logger.Info("MainViewModel|Macro 3: {0}", value);
            }
        }

        /// <summary>
        /// Gets the TXLine property. TXLine is the transmetted G-Code or Grbl commands to the Arduino
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
        /// Get the IsSendingFile property. When sending file we cannot use jog or send manual command.
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
                logger.Info("MainViewModel|Manual sending: {0}", value);
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
                logger.Info("MainViewModel|Manual speed rate value : {0}", value);
            }
        }

        /// <summary>
        /// Sets the maximum feed rate allowed
        /// </summary>
        public double MaxFeedRate { get; private set; } = 1000;

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
                logger.Info("MainViewModel|Manual step value : {0}", value);
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
                logger.Info("MainViewModel|Jog is enabled: {0}", value);
            }
        }
        
        /// <summary>
        /// Gets the ResponseStatus property. This is the current status of the software (Queued, Data received, Ok, Not Ok).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public RespStatus ResponseStatus
        {
            get { return _responseStatus; }
            set { Set(ref _responseStatus, value); }
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
                logger.Info("MainViewModel|Manual laser power value : {0}", value);
            }
        }

        /// <summary>
        /// Sets the maximum laser power allowed.
        /// </summary>
        public double MaxLaserPower { get; private set; } = 100;

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
        /// Get the FileName property. FileName is the G-code file name.
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
                logger.Info("MainViewModel|File name : {0}", value);
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
                logger.Info("MainViewModel|Job time : {0}", value);
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
                logger.Info("MainViewmodel|Number of lines: {0}", value);
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
                logger.Info("MainViewModel|Is sending {0}", value);
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
                logger.Info("MainViewModel|Set Transfer delay : {0}", value);
            }
        }

        /// <summary>
        /// Get the ListTransferDelay property. 
        /// </summary>
        public string[] ListTransferDelay { get; private set; } = { "Slow", "Normal", "Fast" };

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
                logger.Info("MainViewModel|Selected Port : {0}", value);
            }
        }

        /// <summary>
        /// Get the ListPortName property. 
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string[] ListPortNames { get; private set; }

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
                logger.Info("MainViewModel|Selected Baudrate : {0}", value);

            }
        }

        /// <summary>
        /// Get the ListBaudRates property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int[] ListBaudRates { get; private set; } = { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
        #endregion

        #region subregion grbl info
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

        #region subregion coordinate
        /// <summary>
        /// Get the GroupBoxCoordinateTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxCoordinateTitle
        {
            get
            {
                return _groupBoxCoordinateTitle;
            }
            set
            {
                Set(ref _groupBoxCoordinateTitle, value);
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
        #endregion

        #region subregion image
        /// <summary>
        /// Get the GroupBoxImageTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxImageTitle
        {
            get
            {
                return _groupBoxImageTitle;
            }
            set
            {
                Set(ref _groupBoxImageTitle, value);
            }
        }

        /// <summary>
        /// Get the ImagePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                Set(ref _imagePath, value);
                logger.Info("MainViewModel|Image path : {0}", value);

            }
        }

        /// <summary>
        /// Get the ImgSource property.
        /// </summary>
        public BitmapSource ImgSource { get; set; }

        /// <summary>
        /// Get the ImgTransform property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public BitmapSource ImgTransform
        {
            get
            {
                return _imgTransform;
            }
            set
            {
                Set(ref _imgTransform, value);
                logger.Info("MainViewModel|Image transform");

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
            try
            {
                logger.Info("---Program started ! ---");
                _dataService = dataService;
                _dataService.GetSerialPortSetting(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("MainViewModel|Exception GetPortSettings raised: " + error);
                            return;
                        }
                        logger.Info("MainViewModel|Load serial port window");
                        _serialPort = new SerialPort();
                        GroupBoxPortSettingTitle = item.SerialPortSettingHeader;
                        GetSerialPortSettings();
                    });

                _dataService.GetGrbl(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("MainViewModel|Exception GrblSetting raised: " + error);
                            return;
                        }
                        logger.Info("MainViewModel|Load Grbl windows");
                        GroupBoxGrblSettingTitle = item.GrblSettingHeader;
                        GroupBoxGrblConsoleTitle = item.GrblConsoleHeader;
                        GroupBoxGrblCommandTitle = item.GrblCommandHeader;
                    });

                _dataService.GetGCode(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("MainViewModel|Exception GCodeFile raised: " + error);
                            return;
                        }
                        logger.Info("MainViewModel|Load GCode window");
                        GroupBoxGCodeFileTitle = item.GCodeHeader;
                        SelectedTransferDelay = ListTransferDelay[1];
                    });

                _dataService.GetCoordinate(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("MainViewModel|Exception Coordinate raised: " + error);
                            return;
                        }
                        logger.Info("MainViewModel|Load Coordinate window");
                        GroupBoxCoordinateTitle = item.CoordinateHeader;
                    });

                _dataService.GetImage(
                   (item, error) =>
                   {
                       if (error != null)
                       {
                           logger.Error("MainViewModel|Exception GCodeFile raised: " + error);
                           return;
                       }
                       logger.Info("MainViewModel|Load Image window");
                       GroupBoxImageTitle = item.ImageHeader;
                   });
                DefaultPortSettings();
                MyRelayCommands();
                InitializeDispatcherTimer();
                logger.Info("MainViewModel|MainWindow initialization finished");
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception MainViewModel raised: " + ex.ToString());
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// List of RelayCommands bind to button in ViewModels
        /// </summary>
        private void MyRelayCommands()
        {
            ConnectCommand = new RelayCommand(OpenSerialPort, CanExecuteOpenSerialPort);
            DisconnectCommand = new RelayCommand(CloseSerialPort, CanExecuteCloseSerialPort);
            RefreshPortCommand = new RelayCommand(RefreshSerialPort, CanExecuteRefreshSerialPort);
            SendCommand = new RelayCommand(SendData, CanExecuteSendData);
            SendM1Command = new RelayCommand(SendM1Data, CanExecuteSendM1Data);
            SendM2Command = new RelayCommand(SendM2Data, CanExecuteSendM2Data);
            SendM3Command = new RelayCommand(SendM3Data, CanExecuteSendM3Data);
            SendM4Command = new RelayCommand(SendM4Data, CanExecuteSendM4Data);
            ClearCommand = new RelayCommand(ClearData, CanExecuteClearData);
            GrblResetCommand = new RelayCommand(GrblReset, CanExecuteRealTimeCommand);
            GrblPauseCommand = new RelayCommand(GrblFeedHold, CanExecuteRealTimeCommand);
            GrblCurrentStatusCommand = new RelayCommand(GrblCurrentStatus, CanExecuteRealTimeCommand);
            GrblStartCommand = new RelayCommand(GrblStartCycle, CanExecuteRealTimeCommand);
            GrblSettingsCommand = new RelayCommand(GrblSettings, CanExecuteOtherCommand);
            GrblParametersCommand = new RelayCommand(GrblParameters, CanExecuteOtherCommand);
            GrblParserStateCommand = new RelayCommand(GrblParserState, CanExecuteOtherCommand);
            GrblBuildInfoCommand = new RelayCommand(GrblBuildInfo, CanExecuteOtherCommand);
            GrblStartupBlocksCommand = new RelayCommand(GrblStartupBlocks, CanExecuteOtherCommand);
            GrblCheckCommand = new RelayCommand(GrblCheck, CanExecuteOtherCommand);
            GrblKillAlarmCommand = new RelayCommand(GrblKillAlarm, CanExecuteOtherCommand);
            GrblHomingCommand = new RelayCommand(GrblHoming, CanExecuteOtherCommand);
            GrblSleepCommand = new RelayCommand(GrblSleep, CanExecuteOtherCommand);
            GrblTestCommand = new RelayCommand(GrblTest, CanExecuteGrblTest);
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
            JogUpCommand = new RelayCommand<bool>(JogUp, CanExecuteJog);
            JogDownCommand = new RelayCommand<bool>(JogDown, CanExecuteJog);
            StepCommand = new RelayCommand<string>(GetStep,CanExecuteGetStep);
            IncreaseFeedRateCommand = new RelayCommand<bool>(IncreaseFeedRate, CanExecuteFeedRate);
            DecreaseFeedRateCommand = new RelayCommand<bool>(DecreaseFeedRate, CanExecuteFeedRate);
            ResetAxisXCommand = new RelayCommand(ResetAxisX, CanExecuteResetAxis);
            ResetAxisYCommand = new RelayCommand(ResetAxisX, CanExecuteResetAxis);
            ResetAxisZCommand = new RelayCommand(ResetAxisY, CanExecuteResetAxis);
            ResetAllAxisCommand = new RelayCommand(ResetAxisZ, CanExecuteResetAxis);
            StartLaserCommand = new RelayCommand(StartLaser, CanExecuteLaser);
            StopLaserCommand = new RelayCommand(StopLaser, CanExecuteLaser);
            LoadFileCommand = new RelayCommand(OpenFile, CanExecuteOpenFile);
            LoadImageCommand = new RelayCommand(OpenImage, CanExecuteOpenImage);
            //StopFileCommand = new RelayCommand(StopFile, CanExecuteStopFile);
            SendFileCommand = new RelayCommand(StartSendingFileAsync, CanExecuteAsyncTask);
            StopFileCommand = new RelayCommand(StopSendingFileA, CanExecuteAsyncTask);

            IncreaseLaserPowerCommand = new RelayCommand<bool>(IncreaseLaserPower, CanExecuteLaserPower);
            DecreaseLaserPowerCommand = new RelayCommand<bool>(DecreaseLaserPower, CanExecuteLaserPower);
            logger.Info("MainViewModel|All RelayCommands loaded");
        }

        #region subregion serial port method
        /// <summary>
        /// Gets serial port settings from SerialPortSettingsModel class
        /// </summary>
        /// <param name="settingsInit"></param>
        public void GetSerialPortSettings()
        {
            try
            {
                ListPortNames = SerialPort.GetPortNames();
                logger.Info("MainViewModel|Get serial port names");
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception GetSerialPortSettings raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Set default settings for serial port
        /// </summary>
        public void DefaultPortSettings()
        {
            try
            {
                SelectedBaudRate = 115200;
                if (ListPortNames != null && ListPortNames.Length > 0)
                {
                    SelectedPortName = ListPortNames[0];
                }
                else
                {
                    logger.Info("MainViewModel|No port COM available");
                }
                logger.Info("MainViewModel|Serial port default settings loaded");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "MainViewModel|Exception DefaultPortSettings raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Reload serial port settings and set default values.
        /// </summary>
        public void RefreshSerialPort()
        {
            logger.Info("MainViewModel|Refresh serial port");
            GetSerialPortSettings();
            DefaultPortSettings();
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
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.Open();
                logger.Info("MainViewModel|Port COM open");
                CheckCommunication();
            }
            catch (Exception ex)
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

        public void CheckCommunication()
        {
            if(_serialPort.IsOpen)
            {
                GrblReset(); //Do a soft reset before starting a new job?
                GrblBuildInfo();
                //Thread.Sleep(1000);
                if (VersionGrbl != "0.9" || VersionGrbl != "1.1")
                {
                    //CloseSerialPort();
                    logger.Info("MainViewModel| Unknown device or Bad Grbl version {0}", VersionGrbl);
                }
            }
            else
            {
                CloseSerialPort();
                logger.Info("MainViewModel| Wrong com port", VersionGrbl);
            }
        }

        /// <summary>
        /// End serial port communication
        /// </summary>
        public void CloseSerialPort()
        {
            try
            {
                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.Dispose();
                _serialPort.Close();
                Cleanup();
                logger.Info("MainViewModel|Port COM closed");
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception CloseSerialPort raised: " + ex.ToString());
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
                logger.Info("MainViewModel|Data TX: {0}", TXLine);
                TXLine = string.Empty;
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception SendData raised: " + ex.ToString());
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
                _serialPort.WriteLine(Macro1);
                logger.Info("MainViewModel|Data TX: {0}", Macro1);
        }
        public bool CanExecuteSendM1Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro1))
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
            _serialPort.WriteLine(Macro2);
            logger.Info("MainViewModel|Data TX: {0}", Macro2);
        }
        public bool CanExecuteSendM2Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro2))
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
            _serialPort.WriteLine(Macro3);
            logger.Info("MainViewModel|Data TX: {0}", Macro3);
        }
        public bool CanExecuteSendM3Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro3))
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
            _serialPort.WriteLine(Macro4);
            logger.Info("MainViewModel|Data TX: {0}", Macro4);
        }
        public bool CanExecuteSendM4Data()
        {
            if (_serialPort.IsOpen && !string.IsNullOrEmpty(Macro4))
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
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Write(new byte[1] { b }, 0, 1);
                    if (b != 63)//Skips current status logger with DispatcherTimer
                    {
                        logger.Info("MainViewModel|Method WriteByte: {0}", b);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception WriteByte raised: " + ex.ToString());
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
                    logger.Info("MainViewModel|Method WriteBytes: {0}", buffer.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception WriteBytes raised: " + ex.ToString());
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
                    logger.Info("MainViewModel|Method WriteString: {0}", data);
                }
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception WriteString raised: " + ex.ToString());
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
                GCodeLine = string.Empty;
                FileQueue.Clear();
                ListGrblSetting.Clear();
                ListConsoleData.Clear();
                NLine = 0;
                PercentLine = 0;
                RLine = 0;
                EstimateJobTime = "00:00:00" ;
                logger.Info("MainViewModel|TXLine/RXLine and buffers erased");
            }
            catch (Exception ex)
            {
                logger.Error("MainViewModel|Exception ClearData raised: " + ex.ToString());
            }
        }
        /// <summary>
        /// Allow/Disallow the cleardata method to be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteClearData()
        {
            if (Buf!="0")
            {
                return false;
            }
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
            logger.Info("MainViewModel|Soft reset");
        }
       
        /// <summary>
        /// Writes Grbl "~" real-time command (ascii dec 126) to start the machine after a pause or 'M0' command.
        /// </summary>
        public void GrblStartCycle()
        {
            WriteByte(126);
            logger.Info("MainViewModel|Start machin");
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
        /// Sends the Grbl '$$' command to get all particular $x=var settings of the machine
        /// </summary>
        public void GrblSettings()
        {
            ListGrblSetting.Clear();
            WriteString("$$");
            Thread.Sleep(100);//Waits for ListSettingModel to populate all setting values
            SettingCollection = new ObservableCollection<GrblModel>(ListGrblSetting);
            logger.Info("MainViewModel|Grbl settings");
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
        /// Write Grbl '$H' command to run homing cycle.
        /// </summary>
        public void GrblHoming()
        {
            WriteString("$H");
            logger.Info("MainViewModel|Grbl homing");
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
            if (_serialPort.IsOpen)
            {
                if (ResponseStatus != RespStatus.NOk)
                {
                    return true;
                }
            }
            return false;
            //return true;
        }
        #endregion

        #region subregion G-code commands
        /// <summary>
        /// Rapid move to home position.
        /// </summary>
        public void JogH(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(0,0, 0, 0, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogH: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Y+
        /// </summary>
        public void JogN(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,1, 0, 1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogN: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Y-
        /// </summary>
        public void JogS(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,1, 0, -1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogS: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X+
        /// </summary>
        public void JogE(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,1, 1, 0, 0, FeedRate, Double.Parse(Step.Replace('.',',')));
            WriteString(line);
            logger.Info("MainViewModel|JogE: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X-
        /// </summary>
        public void JogW(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,1, -1, 0, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogW: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X- Y+
        /// </summary>
        public void JogNW(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,1, -1, 1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogNW: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X+ Y+
        /// </summary>
        public void JogNE(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,1, 1, 1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogNE: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X- Y-
        /// </summary>
        public void JogSW(bool parameter)
        {
            string line =gcodeToolBasic.FormatGcode(1,1, -1, -1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogSW: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step X+ Y-
        /// </summary>
        public void JogSE(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,1, 1, -1, 0, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogSE: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Z+
        /// </summary>
        public void JogUp(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,0, 0, 0, 1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogUp: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Move one step Z-
        /// </summary>
        public void JogDown(bool parameter)
        {
            string line = gcodeToolBasic.FormatGcode(1,0, 0, 0, -1, FeedRate, Double.Parse(Step.Replace('.', ',')));
            WriteString(line);
            logger.Info("MainViewModel|JogDown: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Allows jogging mode if keyboard is not checked.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteJog(bool parameter)
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk && VersionGrbl.StartsWith("0"))
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
                if (!string.IsNullOrEmpty(parameter))
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
            logger.Info("MainViewModel|F{0}", _feedRate);
        }

        /// <summary>
        /// Decrease the motion speed with keyboard
        /// </summary>
        /// <param name="parameter"></param>
        private void DecreaseFeedRate(bool parameter)
        {
            FeedRate -= 10;
            logger.Info("MainViewModel|F{0}", _feedRate);
        }

        /// <summary>
        /// Allows/Disallows feed rate motion increase/decrease methods.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CanExecuteFeedRate(bool parameter)
        {
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk)
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
            return (_serialPort.IsOpen);
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
            if (_serialPort.IsOpen && ResponseStatus != RespStatus.NOk)
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
            string line = "G10 P0 L2 X0";
            WriteString(line);
            logger.Info("MainViewModel|Reset X: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Sets current axis Y to 0.
        /// </summary>
        public void ResetAxisY()
        {
            string line = "G10 P0 L20 Y0";
            WriteString(line);
            logger.Info("MainViewModel|Reset Y: {0}", line);
        }

        /// <summary>
        /// Sets current axis Z to 0.
        /// </summary>
        public void ResetAxisZ()
        {
            string line = "G10 P0 L20 Z0";
            WriteString(line);
            logger.Info("MainViewModel|Reset Z: {0}", line);
            TXLine = line;
        }

        /// <summary>
        /// Sets all current axis to 0.
        /// </summary>
        public void ResetAllAxis()
        {
            string line = "G10 P0 L20 X0 Y0 Z0";
            WriteString(line);
            logger.Info("MainViewModel|Reset All axis: {0}", line);
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

        #region subregion other methods
        /// <summary>
        /// Does nothing yet...
        /// </summary>
        public override void Cleanup()
        {
            // Clean up if needed
            currentStatusTimer.Stop();
            base.Cleanup();
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
            if(FileQueue!=null)
            {
                //FileQueue.Clear();
                ClearData();
            }
            OpenFileDialog openFile = new OpenFileDialog();
            try
            {
                openFile.Title = "Fichier G-code";
                openFile.Filter = "G-Code files|*.txt;*.gcode;*.ngc;*.nc,*.cnc|Tous les fichiers|*.*";
                openFile.FilterIndex = 1;
                openFile.DefaultExt = ".txt";
                if(openFile.ShowDialog().Value)
                {
                    FileName = openFile.FileName;
                    LoadFile(FileName);
                    logger.Info("MainViewModel|OpenFile: {0}", FileName);
                }
            }
            catch(Exception ex)
            {
                logger.Error("MainViewModel|Exception data received raised: " + ex.ToString());
                ResponseStatus = RespStatus.NOk;
            }
        }

        /// <summary>
        /// Allows/Disallows LoadFile method.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteOpenFile()
        {
            return true;
        }

        /// <summary>
        /// Load G-Code file and save lines to a queue and a list.
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadFile(string fileName)
        {
            string line;
            FileQueue.Clear();
            FileList.Clear();
            using (StreamReader sr = new StreamReader(fileName))
            {
                while((line=sr.ReadLine())!=null)
                {
                    FileQueue.Enqueue(gcodeToolBasic.TrimGcode(line));
                    FileList.Add(line);
                    GCodeLine += line + Environment.NewLine;
                }
            }
            logger.Info(GCodeLine);
            NLine = FileQueue.Count;
            RLine = NLine;
            Tools.GCodeTool gcodeTool = new Tools.GCodeTool(FileList);
            TimeSpan time = TimeSpan.FromSeconds(Math.Round(gcodeTool.CalculateJobTime(MaxFeedRate)));
            EstimateJobTime = time.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Send the G-Code file in async mode.
        /// </summary>
        private async void StartSendingFileAsync()
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            var progressHandler = new Progress<double>(value => PercentLine = value);
            var progress = progressHandler as IProgress<double>;
            var t = Task.Run(() =>
                {
                    for (int i = 0; i <= NLine; ++i)
                    {
                        SendFile();
                        logger.Info("MainViewModel|N{0}: {1}", i.ToString(),TXLine);
                        if (progress != null && NLine != 0)
                        {
                            progress.Report((NLine - RLine) / NLine);
                        }
                        else
                        {
                            progress.Report(0);
                        }
                        Thread.Sleep(transferDelay);
                        if(!IsSending)
                        {
                            cts.Cancel();
                        }
                        if (token.IsCancellationRequested)
                        {
                            logger.Info("MainViewModel|Sending file canceled at line {0}", i.ToString());
                            break;
                        }
                    }
                },token);
            switch (SelectedTransferDelay)
            {
                case "Slow":
                    transferDelay = 2000;
                    break;
                case "Normal":
                    transferDelay = 750;
                    break;
                case "Fast":
                    transferDelay = 250;
                    break;
            }
            logger.Info("MainViewModel|Transfer speed:", SelectedTransferDelay);
            try
            {
                await t;
                logger.Info("MainViewModel|Task sending file completed");
            }

            catch (AggregateException)
            {
                logger.Info("MainViewModel|Task sending file cancelled");
            }

            finally
            {
                cts.Dispose();
            }
        }

        /// <summary>
        /// Cancel the task to send G-code file in async. Used with stop button.
        /// </summary>
        private void StopSendingFileA()
        {
            if(cts!=null)
            {
                logger.Info("MainViewModel|Task sending file cancelled");
                cts.Cancel();
                GrblFeedHold();
                FileQueue.Clear();
            }
        }

        /// <summary>
        /// Allows/Disallows SendFile method.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteAsyncTask()
        {
            if (FileQueue.Count > 0 && _serialPort.IsOpen)
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
            //string line = string.Empty;
            RLine = FileQueue.Count;
            if (RLine > 0 && (int)ResponseStatus != 1)
            {
                if (MachineStatus != MachStatus.Alarm)
                {
                    ResponseStatus = RespStatus.Q;
                    TXLine = FileQueue.Dequeue();
                    WriteString(TXLine);
                    IsManualSending = false;
                    IsSending = true;
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

        /// <summary>
        /// Allows/Disallows SendFile method.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteSendFile()
        {
            if (FileQueue.Count>0 && _serialPort.IsOpen)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes status response pause sending file
        /// </summary>
        public void StopFile()
        {
            ResponseStatus = RespStatus.Q;
            FileQueue.Clear();
            NLine = FileQueue.Count;
            IsManualSending = false;
        }

        /// <summary>
        /// Allows/Disallows PauseFile method.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteStopFile()
        {
            if (FileQueue.Count > 0)
                {
                return true;
                }
            return false;
        }
        #endregion

        #region subregion Image
        private void OpenImage()
        {
            OpenFileDialog openImage = new OpenFileDialog();
            openImage.DefaultExt = ".jpeg";
            openImage.Filter = "Images |*.jpeg;*.jpeg;*.png;*.bmp";
            try
            {
                if(openImage.ShowDialog().Value&&openImage.FileName.Length>0)
                {
                    ImagePath = openImage.FileName;
                    logger.Info("MainViewModel|Load image, filename: {0}", FileName);
                    LoadImage(ImagePath);
                }
            }
            catch(Exception ex)
            {
                logger.Info("MainViewModel|Exception LoadImage raised: {0}", ex.ToString());
                ResponseStatus = RespStatus.NOk;
            }
        }

        private bool CanExecuteOpenImage()
        {
            return true;
        }

        public void LoadImage(string fileName)
        {
            
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
        #endregion

        #region Event
        /// <summary>
        /// Get all data from serial port and process response.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = _serialPort.ReadLine();
                RXLine = gcodeToolBasic.TrimGcode(line);
                GrblModel = new GrblModel(TXLine, RXLine);
                ConsoleData = new ObservableCollection<GrblModel>(ListConsoleData);
                if (IsVerbose)
                {
                    ListConsoleData.Add(GrblModel);
                }
                else if (!line.StartsWith("<"))
                {
                    ListConsoleData.Add(GrblModel);
                }
                grbltool.DataGrblSorter(line);
                ResponseStatus = (RespStatus)grbltool.ResponseStatus;
                MachineStatus = (MachStatus)grbltool.MachineStatus;
                MachineStatusColor = (SolidColorBrush)grbltool.MachineStatusColor;
                MPosX = grbltool.MachinePositionX;
                MPosY = grbltool.MachinePositionY;
                WPosX = grbltool.WorkPositionX;
                WPosY = grbltool.WorkPositionY;
                Buf = grbltool.PlannerBuffer;
                RX = grbltool.RxBuffer;
                VersionGrbl = grbltool.VersionGrbl;
                BuildInfoGrbl = grbltool.BuildInfo;
                InfoMessage = grbltool.InfoMessage;
                if(ListConsoleData.Count>4)//Number of lines showed in data console
                {
                    ListConsoleData.RemoveAt(0);
                }
                ListGrblSetting = grbltool.ListGrblSettingModel;
            }
            catch (Exception ex)
                {
                logger.Error("MainViewModel|Exception data received raised: " + ex.ToString());
                ResponseStatus = RespStatus.NOk;
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
                    logger.Info("MainViewModel|Port COM disposed");
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