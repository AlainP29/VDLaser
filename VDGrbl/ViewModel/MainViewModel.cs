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
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VDGrbl.Codes;
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

        private string _selectedPortName = string.Empty;
        private string _versionGrbl = "-", _buildInfo = "-";
        private string _mposX = "0.000", _mposY = "0.000";
        private string _wposX = "0.000", _wposY = "0.000";
        private string _step = "1";
        private string _buf = "0", _rx = "0";
        private string _errorMessage = string.Empty;
        private string _alarmMessage = string.Empty;
        private string _infoMessage = string.Empty;
        private string _fileName = string.Empty;
        private string _estimateJobTime = "00:00:00";
        private string _groupBoxPortSettingTitle = string.Empty;
        private string _groupBoxGrblSettingTitle = string.Empty;
        private string _groupBoxGrblConsoleTitle = string.Empty;
        private string _groupBoxGrblCommandTitle = string.Empty;
        private string _groupBoxGCodeTitle = string.Empty;
        private string _groupBoxCoordinateTitle = string.Empty;

        private string _txLine = string.Empty;
        private string _rxLine = string.Empty;
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

        private SerialPort _serialPort;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        DispatcherTimer currentStatusTimer = new DispatcherTimer(DispatcherPriority.Normal);

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
        public RelayCommand SendFileCommand { get; private set; }
        public RelayCommand StopFileCommand { get; private set; }
        public RelayCommand<bool> DecreaseLaserPowerCommand { get; private set; }
        public RelayCommand<bool> IncreaseLaserPowerCommand { get; private set; }
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
        /// Gets the SelectedPortName property.
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
            }
        }

        /// <summary>
        /// Get the ListParities property.
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
            }
        }

        /// <summary>
        /// Get the ListBaudRates property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int[] ListBaudRates { get; private set; } = { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
        #endregion

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

        #region subregion GrblModel
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
        #endregion

        #region subregion GCodeFileModel
        /// <summary>
        /// Get the GroupBoxGCodeFileTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxGCodeTitle
        {
            get
            {
                return _groupBoxGCodeTitle;
            }
            set
            {
                Set(ref _groupBoxGCodeTitle, value);
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
        /// Get the IsSending property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSending {
            get
            {
                return _isSending;
            }
            set
            {
                Set(ref _isSending, value);
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
        /// The <see cref="Macro1" /> property's name.
        /// </summary>
        public const string Macro1PropertyName = "Macro1";
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
            }
        }

        /// <summary>
        /// The <see cref="Macro2" /> property's name.
        /// </summary>
        public const string Macro2PropertyName = "Macro2";
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
            }
        }

        /// <summary>
        /// The <see cref="Macro3" /> property's name.
        /// </summary>
        public const string Macro3PropertyName = "Macro3";
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
            }
        }

        /// <summary>
        /// The <see cref="Macro4" /> property's name.
        /// </summary>
        public const string Macro4PropertyName = "Macro4";
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
        /// The <see cref="ListGrblSetting" /> property's name.
        /// </summary>
        public const string ListSettingModelName = "ListSettingModel";
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
        /// The <see cref="SettingCollection" /> property's name.
        /// </summary>
        public const string SettingCollectionName = "SettingCollection";
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

        /// <summary>
        /// The <see cref="Buf" /> property's name.
        /// </summary>
        public const string BufPropertyName = "Buf";
        /// <summary>
        /// Gets the Buf property. Buf is the Number of motions queued in Grbl's planner buffer.
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
        /// The <see cref="RX" /> property's name.
        /// </summary>
        public const string RXPropertyName = "RX";
        /// <summary>
        /// Gets the RX property. RX is the Number of characters queued in Grbl's serial RX receive buffer.
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
        /// The <see cref="ErrorMessage" /> property's name.
        /// </summary>
        public const string ErrorMessagePropertyName = "ErrorMessage";
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
        /// The <see cref="AlarmMessage" /> property's name.
        /// </summary>
        public const string AlarmMessagePropertyName = "AlarmMessage";
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
        /// The <see cref="InfoMessage" /> property's name.
        /// </summary>
        public const string InfoMessagePropertyName = "InfoMessage";
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
        /// Gets the FileName property. FileName is the G-code file name.
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
            }
        }

        /// <summary>
        /// The <see cref=" EstimateJobTime" /> property's name.
        /// </summary>
        public const string EstimateJobTimePropertyName = "EstimateJobTime";
        /// <summary>
        /// Gets the EstimateJobTime property. EstimateJobTime is the estimation of running time.
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
            }
        }

        /// <summary>
        /// The <see cref="IsVerbose" /> property's name.
        /// </summary>
        public const string IsVerbosePropertyName = "IsVerbose";
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

        /// <summary>
        /// Gets the FileQueue property. FileQueue is populated w/ lines of G-code file trimed.
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
            }
        }

        /// <summary>
        /// Gets the FileList property. FileList is populated w/ lines of G-code file.
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
            }
        }

        /// <summary>
        /// The <see cref="NLine" /> property's name.
        /// </summary>
        public const string NLinePropertyName = "NLine";
        /// <summary>
        /// Gets the NLine property. NLine is the number of lines in the G-code file.
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
            }
        }

        /// <summary>
        /// The <see cref="RLine" /> property's name.
        /// </summary>
        public const string RLinePropertyName = "RLine";
        /// <summary>
        /// Gets the RLine property. RLine is the number of lines remaning in the G-code queue.
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
        /// The <see cref="PercentLine" /> property's name.
        /// </summary>
        public const string PercentLinePropertyName = "PercentLine";
        /// <summary>
        /// Gets the PercentLine property. PercentLine is the number of lines remaning in the G-code queue.
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
        /// The <see cref="ListConsoleData" /> property's name.
        /// </summary>
        public const string ListConsoleDataPropertyName = "ListConsoleData";
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
        /// The <see cref="ConsoleData" /> property's name.
        /// </summary>
        public const string ConsoleDataPropertyName = "ConsoleData";
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
        /// The <see cref="IsManualSending" /> property's name.
        /// </summary>
        public const string IsManualSendingPropertyName = "IsManualSending";
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
                logger.Info("MainViewModel|Manual speed rate value is {0}", _feedRate);
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
                logger.Info("MainViewModel|Manual step value is {0}", value);
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
                    logger.Info("MainViewModel|Keyboard is selected");

                }
                else
                {
                logger.Info("MainViewModel|Keyboard is not selected");
            }
            }
        }

        /// <summary>
        /// The <see cref="IsSelectedMetric" /> property's name.
        /// </summary>
        public const string IsSelectedMetricPropertyName = "IsSelectedMetric";
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
                if(_isSelectedMetric == false && _serialPort.IsOpen)
                {
                    WriteString("G20");
                    logger.Info("MainViewModel|Metric is not selected");
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

        /// <summary>
        /// The <see cref="LaserPower" /> property's name.
        /// </summary>
        public const string LaserPowerPropertyName = "LaserPower";
        /// <summary>
        /// Gets the LaserPower property. LaserPower is the power of the laser.
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
                WriteString(string.Format("S{0}",_laserPower));
                logger.Info("MainViewModel|Manual laser power value is {0}", _laserPower);
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
        /// The <see cref="MachineStatusColor" /> property's name.
        /// </summary>
        public const string LaserColorPropertyName = "LaserColor";
        /// <summary>
        /// Gets the LaserColor property. The color change depending of the current state of the laser (ON=Blue, OFF=Light Gray...)
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
        /// The <see cref="MPosX" /> property's name.
        /// </summary>
        public const string MPosXPropertyName = "MPosX";
        /// <summary>
        /// Gets the PosX property. PosX is the X coordinate of the machin get w/ '?' Grbl real-time command.
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
        /// The <see cref="MPosY" /> property's name.
        /// </summary>
        public const string MPosYPropertyName = "MPosY";
        /// <summary>
        /// Gets the PosY property. PosY is the Y coordinate received the machin get w/ '?' Grbl real-time command.
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
        /// The <see cref="WPosX" /> property's name.
        /// </summary>
        public const string WPosXPropertyName = "WPosX";
        /// <summary>
        /// Gets the PosX property. PosX is the X coordinate of the machin get w/ '?' Grbl real-time command.
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
        /// The <see cref="WPosY" /> property's name.
        /// </summary>
        public const string WPosYPropertyName = "WPosY";
        /// <summary>
        /// Gets the PosY property. PosY is the Y coordinate received the machin get w/ '?' Grbl real-time command.
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
                        GroupBoxGCodeTitle = item.GCodeHeader;
                    });

                _dataService.GetCoordinate(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("MainViewModel|Exception GCodeFile raised: " + error);
                            return;
                        }
                        GroupBoxCoordinateTitle = item.CoordinateHeader;
                    });

                _dataService.GetCoordinate(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("MainViewModel|Exception GCodeFile raised: " + error);
                            return;
                        }
                        GroupBoxCoordinateTitle = item.CoordinateHeader;
                    });
                DefaultPortSettings();
                MyRelayCommands();
                InitializeDispatcherTimer();
                logger.Info("MainViewModel|MainWindow initialized");
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
            SendFileCommand = new RelayCommand(SendFile, CanExecuteSendFile);
            StopFileCommand = new RelayCommand(StopFile, CanExecuteStopFile);
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
                logger.Info("MainViewModel|All serial port settings loaded");
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
                if (ListPortNames != null && ListPortNames.Length > 0)
                {
                    SelectedPortName = ListPortNames[0];
                    SelectedBaudRate = 115200;
                    logger.Info("MainViewModel|Default settings loaded");
                }
                else
                {
                    logger.Info("MainViewModel|No port COM available");
                }

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
            GetSerialPortSettings();
            DefaultPortSettings();
            logger.Info("MainViewModel|Refresh Port COM");
        }
        /// <summary>
/// Allows/Disallows RefreshSerialPort method to be executed.
/// </summary>
/// <returns></returns>
        public bool CanExecuteRefreshSerialPort()
        {
                return true;
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
                    GrblReset(); //Do a soft reset before starting a new job?
                    GrblBuildInfo();
                    logger.Info("MainViewModel|Port COM open");
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
        }
       
        /// <summary>
        /// Writes Grbl "~" real-time command (ascii dec 126) to start the machine after a pause or 'M0' command.
        /// </summary>
        public void GrblStartCycle()
        {
            WriteByte(126);
        }
        
        /// <summary>
        /// Writes Grbl "!" real-time command (ascii dec 33) to pause the machine motion X, Y and Z (not spindle or laser).
        /// </summary>
        public void GrblFeedHold()
        {
            WriteByte(33);
        }

        /// <summary>
        /// Writes Grbl "?" real-time command "?" (Ascii dec 63) to get immadiate status report of the machine.
        /// </summary>
        public void GrblCurrentStatus()
        {
            WriteByte(63);
            ResponseStatus = (RespStatus)grbltool.ResponseStatus;
            MachineStatus = (MachStatus)grbltool.MachineStatus;
            MachineStatusColor = (SolidColorBrush)grbltool.MachineStatusColor;
        }

        /// <summary>
        /// Write Grbl '$X' command to kill alarm mode. In real-time command and canexecuterealTimeCommand in order to kill alarm
        /// </summary>
        public void GrblKillAlarm()
        {
            WriteString("$X");
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
        }

        /// <summary>
        /// Writes Grbl '$#' command to view parameters [G54:0.000,0.000,0.000].
        /// </summary>
        public void GrblParameters()
        {
            WriteString("$#");
        }

        /// <summary>
        /// Writes Grbl '$G' command to view G-code parser state [G0 G54 G17 G21 G90 G94 M0 M5 M9 T0 F0. S0.].
        /// </summary>
        public void GrblParserState()
        {
            WriteString("$G");
        }

        /// <summary>
        /// Writes Grbl '$I' command to view version and date of build [0.9i.20150620:].
        /// </summary>
        public void GrblBuildInfo()
        {
            WriteString("$I");
        }

        /// <summary>
        /// Write Grbl '$N' command to view startup blocks $N0=.
        /// </summary>
        public void GrblStartupBlocks()
        {
            WriteString("$N");
        }

        /// <summary>
        /// Write Grbl '$C' command to enable check mode (No motion).
        /// </summary>
        public void GrblCheck()
        {
            WriteString("$C");
        }

        /// <summary>
        /// Write Grbl '$H' command to run homing cycle.
        /// </summary>
        public void GrblHoming()
        {
            WriteString("$H");
        }

        /// <summary>
        /// Write Grbl '$SLP' command to enable sleep mode
        /// </summary>
        public void GrblSleep()
        {
            WriteString("$SLP");
        }

        /// <summary>
        /// Write Grbl '$' command to get help.
        /// </summary>
        public void GrblHelp()
        {
            WriteString("$");
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

            base.Cleanup();
        }

        /// <summary>
        /// Initializes DispatcherTimer to query Grbl report state at 4Hz (5Hz is max recommended).
        /// </summary>
        private void InitializeDispatcherTimer()
        {
            currentStatusTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            //dispatcherTimer.Interval = TimeSpan.FromSeconds(0.25);
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
                FileQueue.Clear();
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                openFileDialog.Title = "Fichier G-code";
                openFileDialog.Filter = "G-Code files|*.txt;*.gcode;*.ngc;*.nc,*.cnc|Tous les fichiers|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.DefaultExt = ".txt";
                if(openFileDialog.ShowDialog().Value)
                {
                    FileName = openFileDialog.FileName;
                    LoadFile(FileName);
                    logger.Info("MainViewModel|Open file: {0}", FileName);
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
        /// Loads file
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadFile(string fileName)
        {
            string line;
            FileQueue.Clear();
            FileList.Clear();
            using (StreamReader sr = new StreamReader(FileName))
            {
                while((line=sr.ReadLine())!=null)
                {
                    FileQueue.Enqueue(gcodeToolBasic.TrimGcode(line));
                    FileList.Add(line);
                }
            }
            NLine = FileQueue.Count;
            RLine = _nLine;
            Tools.GCodeTool gcodeTool = new Tools.GCodeTool(FileList);
            TimeSpan time = TimeSpan.FromSeconds(Math.Round(gcodeTool.CalculateJobTime(MaxFeedRate)));
            EstimateJobTime = time.ToString(@"hh\:mm\:ss");
            //EstimateJobTime = time.ToString(@"hh\:mm\:ss\:fff");
        }

        /// <summary>
        /// Sends G-code file line by line =>startSendingFile
        /// </summary>
        public void SendFile()
        {
            string line = string.Empty;
            RLine = FileQueue.Count;
            if (_nLine != 0)//Divide by zero (NaN)
            {
                PercentLine = (_nLine - _rLine) / _nLine;
            }
            else
            {
                PercentLine = 0;
            }
            if (RLine > 0 && (int)ResponseStatus != 1)
            {
                ResponseStatus = RespStatus.Q;
                TXLine = FileQueue.Dequeue();
                WriteString(TXLine);
                IsManualSending = false;
                IsSending = true;
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
            if (FileQueue.Count > 0 && _serialPort.IsOpen)
                {
                return true;
            }
            return false;
        }

        #region TODO use task to send file but pb of synchro showing data...
        /*
        /// <summary>
        /// Starts sending file with button start
        /// </summary>
        public async void StartSendingFile()
        {
            await Task.Run(() => SendingFile());
        }

        /// <summary>
        /// Loop sendFile untill isSending is false
        /// </summary>
        private void SendingFile()
        {
            while(IsSending)
            {
                Thread.Sleep(500);
                SendFile();
            }
        }*/
        #endregion
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
                BuildInfo = grbltool.BuildInfo;
                if(ListConsoleData.Count>5)
                {
                    ListConsoleData.RemoveAt(0);
                    //ListConsoleData.Reverse();

                }
                ListGrblSetting = grbltool.ListGrblSettingModel;
                if(grbltool.CanSend && IsSending)//Use datareceived to send next line should use a background task to send data...
                {
                    
                    SendFile();
                }
                else
                {
                    RXLine = string.Empty;
                }
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