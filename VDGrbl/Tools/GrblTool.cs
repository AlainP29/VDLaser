using System;
using NLog;
using VDGrbl.Codes;
using VDGrbl.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Globalization;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Tools to parse grbl data send or received
    /// </summary>
    public class GrblTool
    {
        #region Fields
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly GrblSettingCodes GrblSettingCode = new GrblSettingCodes();
        private readonly ErrorCodes GrblErrorCode = new ErrorCodes();
        private readonly AlarmCodes GrblAlarmCodec = new AlarmCodes();

        #region subregion enum
        /// <summary>
        /// Enumeration of the response states. Ok: All is good, NOk: Alarm state Q: Queued [DR: Data received] 
        /// </summary>
        public enum RespStatus { Ok, NOk, Q };

        /// <summary>
        /// Enumeration of the machine states.
        /// </summary>
        public enum MachStatus { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep, Undefined };
        #endregion
        #endregion

        #region public property
        public string MachinePositionX { get; private set; } = "0";
        public string MachinePositionY { get; private set; } = "0";
        public string WorkPositionX { get; private set; } = "0";
        public string WorkPositionY { get; private set; } = "0";
        public string OffsetPositionY { get; private set; } = "0";
        public string OffsetPositionX { get; private set; } = "0";
        public string MachineFeed { get; private set; } = "0";
        public string MachineSpeed { get; private set; } = "0";
        public string OverrideMachineFeed { get; private set; } = "0";
        public string OverrideMachineSpeed { get; private set; } = "0";
        public string RxBuffer { get; private set; } = "0";
        public string PlannerBuffer { get; private set; } = "0";
        public string VersionGrbl { get; set; } = "-";
        public string BuildInfo { get; set; } = "-";
        public string AlarmMessage { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;
        public string InfoMessage { get; private set; } = string.Empty;
        public MachStatus MachineStatus { get; private set; }
        public RespStatus ResponseStatus { get; private set; }
        public SolidColorBrush MachineStatusColor { get; private set; }
        public SettingItem GrblSettings {get; private set; }
        public List<SettingItem> ListGrblSettings { get; private set; } = new List<SettingItem>();
        #endregion

        #region Constructor
        public GrblTool()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Sorts Grbl data received like Grbl informations, response, coordinates, settings...
        /// </summary>
        /// <param name="line"></param>
        public void DataGrblSorter(string line)
        {
                if (!string.IsNullOrEmpty(line))
                {
                    string lineTrim = GCodeTool.TrimGcode(line);
                    if (lineTrim.StartsWith("ok", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ProcessResponse(lineTrim);
                    }
                    else if (lineTrim.StartsWith("error", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ProcessErrorResponse(lineTrim);
                    }
                    else if (lineTrim.StartsWith("alarm", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ProcessAlarmResponse(lineTrim);
                    }
                    else if (lineTrim.StartsWith("<", StringComparison.CurrentCulture) && lineTrim.EndsWith(">", StringComparison.CurrentCulture))
                    {
                        ProcessCurrentStatusResponse(lineTrim);
                    }
                    else if (lineTrim.StartsWith("$", StringComparison.CurrentCulture) && lineTrim.Contains("="))
                    {
                        ProcessGrblSettingResponse(lineTrim);
                    }
                    else if (lineTrim.StartsWith("[", StringComparison.CurrentCulture) && lineTrim.EndsWith("]", StringComparison.CurrentCulture))
                    {
                        ProcessInfoResponse(lineTrim);
                    }
                    else if (lineTrim.Contains("rbl"))
                    {
                    ProcessInfoResponse(lineTrim);
                }
                else
                {
                    ProcessInfoResponse(lineTrim);
                }
            }
        }

        /// <summary>
        /// Process Grbl build informations.
        /// </summary>
        /// <param name="data"></param>
        public void ProcessInfoResponse(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                logger.Info(CultureInfo.CurrentCulture, "GrblTool|ProcessInfoResponse|Data:{0}", data);

                ResponseStatus = RespStatus.Ok;
                switch (data.Length)
                {
                    case 9:
                        InfoMessage = "Check gcode mode";//[Enabled]
                        break;

                    case 10:
                        InfoMessage = "Check gcode mode";//[Disabled]
                        break;

                    case 16:
                        VersionGrbl = data.Substring(1, 4);//[0.9i.20150620:]
                        BuildInfo = data.Substring(6, 8);
                        break;

                    case 19:
                        InfoMessage = "Kill alarm lock or reset to continue?";//[Caution: Unlocked] [Reset to continue]
                        break;

                    case 20:
                        VersionGrbl = data.Substring(5, 4);//[Ver:1.1h.20150620:]
                        BuildInfo = data.Substring(10, 8);
                        break;

                    case 21:
                        InfoMessage = "Kill alarm lock or homing to continue";//['$H'|'$X' to unlock]
                        break;

                    case 23 when !data.Contains("PLB")://For C#7 test only :-)
                        InfoMessage = "View G-code parameters";
                        break;

                    case 24:
                        InfoMessage = "View startup blocks"; //Grbl 0.9i ['$' for help] put something like $N0=G20 G54 G17 to get it
                        break;

                    case 44:
                        InfoMessage = "View gcode parser state";//[G0 G54 G17 G21 G90 G94 M0 M5 M9 T0 F0. S0.]
                        break;

                    default:
                        InfoMessage = "Unknown[]";
                        break;
                }
            }
            }

        /// <summary>
        /// Process the serial port ok message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void ProcessResponse(string data)
        {
            ResponseStatus = RespStatus.Ok;
            logger.Info(CultureInfo.CurrentCulture, "GrblTool|ProcessResponse|Data:{0}", data);
        }

        /// <summary>
        /// Process the serial port error message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void ProcessErrorResponse(string data)
        {
            ResponseStatus = RespStatus.Ok;//It is an error but still ok to send next command
            if (!string.IsNullOrEmpty(data))
            {
                if (VersionGrbl.StartsWith("1", StringComparison.InvariantCulture))//In version 1.1 all error codes have ID
                {
                    ErrorMessage = GrblErrorCode.ErrorDict11[data.Split(':')[1]];
                    logger.Error("GrblTool|ProcessErrorResponse|Error key:{0} | description:{1}", data.Split(':')[1], ErrorMessage);
                }
                else if(VersionGrbl.Contains("9"))
                {
                    if (data.Contains("ID"))//In version 0.9 only error code from 23 to 37 have ID
                    {
                        ErrorMessage = GrblErrorCode.ErrorDict09[data.Split(':')[2]];
                        logger.Error("GrblTool|ProcessErrorResponse|Error key {0} | description:{1}", data.Split(':')[2], ErrorMessage);
                    }
                    else//Error codes w/o ID
                    {
                        ErrorMessage = GrblErrorCode.ErrorDict09[data.Split(':')[1]];
                        logger.Error("GrblTool|ProcessErrorResponse|Error key {0} | description:{1}", data.Split(':')[1], ErrorMessage);
                    }
                }
                else
                    logger.Error("GrblTool|ProcessErrorResponse|VersionGrbl not defined or unknown error message");

            }
        }

        /// <summary>
        /// Process the serial port alarm message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void ProcessAlarmResponse(string data)
        {
            ResponseStatus = RespStatus.NOk;
            MachineStatusColor = Brushes.Red;
            MachineStatus = MachStatus.Alarm;
            logger.Info("GrblTool|ProcessResponse|Data:{0}|RespStatus:{1}|MachStatus:{2}", data, ResponseStatus.ToString(), MachineStatus.ToString());
            if(!string.IsNullOrEmpty(data))
            {
                if (VersionGrbl.StartsWith("1",StringComparison.InvariantCulture))
                {
                    AlarmMessage = GrblAlarmCodec.AlarmDict11[data.Split(':')[1]];
                    logger.Info("GrblTool|ProcessAlarmResponse11|Alarm key {0} | description:{1}", data.Split(':')[1], AlarmMessage);
                }
                else
                {
                    AlarmMessage = GrblAlarmCodec.AlarmDict09[data.Split(':')[1]];
                    logger.Info("GrblTool|ProcessAlarmResponse09|Alarm key {0} | description:{1}", data.Split(':')[1], AlarmMessage);

                }
            }
        }

        /// <summary>
        /// Populate the settingsCollection w/ data received w/ Grbl '$$' command.
        /// </summary>
        /// <param name="data"></param>
        public void ProcessGrblSettingResponse(string data)
        {
            if(!string.IsNullOrEmpty(data))
            {
                ResponseStatus = RespStatus.Q;//Wait until we get all settings before sending new line of code
                string[] arr = data.Split(new Char[] { '=', '(', ')', '\r', '\n' });
                if (arr.Length > 0)
                {
                    if (data.Contains("N"))
                    {
                        InfoMessage = "Startup block";//$N0=...
                    }
                    else
                    {
                        if (arr.Length > 3)//Grbl version 0.9 (w/ setting description)
                        {
                            GrblSettings = new SettingItem(arr[0], arr[1], arr[2]);
                            ListGrblSettings.Add(GrblSettings);
                            logger.Info("GrblTool|ProcessGrblSettingResponse|Grbl0.9 settings list: {0}|{1}|{2}", arr[0], arr[1], arr[2]);
                        }
                        else if(arr.Length==2)//Grbl version 1.1 (w/o setting description)
                        {
                            GrblSettings = new SettingItem(arr[0], arr[1], GrblSettingCode.SettingDict[arr[0]]);
                            ListGrblSettings.Add(GrblSettings);
                            logger.Info("GrblTool|ProcessGrblSettingResponse|Grbl1.1 settings list: {0}|{1}|{2}", arr[0], arr[1], GrblSettingCode.SettingDict[arr[0]]);
                        }
                        else
                        {
                            InfoMessage = "Unknown settings";
                            logger.Info("GrblTool|ProcessGrblSettingResponse|Unknown settings");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get coordinates and status depending of Grbl version 0.9 or 1.1.
        /// </summary>
        /// <param name="data"></param>
        public void ProcessCurrentStatusResponse(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                ResponseStatus = RespStatus.Ok;
                if (data.Contains("|") || VersionGrbl.StartsWith("1", StringComparison.InvariantCulture))//Report state Grbl v1.1 < Idle|MPos:0.000,0.000,0.000>
                {
                    string[] arr = data.Split(new Char[] { '<', '>', ',', ':', '\r', '\n', '|' });
                    if (data.Contains("mpos"))
                    {
                        MachinePositionX = arr[3];
                        MachinePositionY = arr[4];
                        WorkPositionX = "0";
                        WorkPositionY = "0";
                    }
                    else
                    {
                        WorkPositionX = arr[3];
                        WorkPositionY = arr[4];
                        MachinePositionX = "0";
                        MachinePositionY = "0";
                    }

                    if (!data.Contains("bf"))
                    {
                        MachineFeed = arr[7];
                        MachineSpeed = arr[8];
                        PlannerBuffer = "0";
                        RxBuffer = "0";
                        if (data.Contains("wco"))
                        {
                            OffsetPositionX = arr[10];
                            OffsetPositionY = arr[11];
                            //Normaly WorkPositionX = MachinePositionX - OffsetPositionX; use a converter string to int first and then calculate...
                        }
                        if (data.Contains("Ov"))
                        {
                            //Override value for feed, rapids and spindle speed
                            OverrideMachineFeed = arr[10];
                            OverrideMachineSpeed = arr[12];
                        }
                    }
                    else
                    {
                        PlannerBuffer = arr[7];
                        RxBuffer = arr[8];
                        MachineFeed = arr[10];
                        MachineSpeed = arr[11];
                        if (data.Contains("wco"))
                        {
                            OffsetPositionX = arr[13];
                            OffsetPositionY = arr[14];
                            //Normaly WorkPositionX = MachinePositionX - OffsetPositionX; use a converter string to int first and then calculate...
                        }
                        if (data.Contains("Ov"))
                        {
                            //Override value for feed, rapids and spindle speed
                            OverrideMachineFeed = arr[13];
                            OverrideMachineSpeed = arr[15];
                        }
                    }

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
                        default:
                            MachineStatus = MachStatus.Undefined;
                            MachineStatusColor = Brushes.DarkGray;
                            break;
                    }
                }
                else//Report state Grbl v0.9 <Idle,MPos:0.000,0.000,0.000,WPos:0.000,0.000,0.000,Buf:0,RX:0>
                {
                    string[] arr = data.Split(new Char[] { '<', '>', ',', ':', '\r', '\n' });

                    if (arr.Length > 13)
                    {
                        RxBuffer = arr[13];
                    }
                    if (arr.Length > 11)
                    {
                        PlannerBuffer = arr[11];
                    }
                    
                    if (arr.Length > 7)
                    {
                        WorkPositionX = arr[7];
                        WorkPositionY = arr[8];
                    }
                    if (arr.Length > 3)
                    {
                        MachinePositionX = arr[3];
                        MachinePositionY = arr[4];
                    }

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
                        default:
                            MachineStatus = MachStatus.Undefined;
                            MachineStatusColor = Brushes.DarkGray;
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
