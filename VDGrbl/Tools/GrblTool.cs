using System;
using NLog;
using VDGrbl.Codes;
using VDGrbl.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VDGrbl.Tools
{
    /// <summary>
    /// usefull tools to parse grbl data send or received
    /// </summary>
    public class GrblTool
    {
        #region Fields
        GCodeTool gcodeTool = new GCodeTool();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        SettingCodes sc = new SettingCodes();
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
        public string VersionGrbl { get; set; } = "0.0";
        public string BuildInfo { get; set; } = "0";
        public string AlarmMessage { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;
        public string InfoMessage { get; private set; } = string.Empty;
        public bool CanSend { get; set; } = false;//replace sendFile =<need to be implemented in MainViewModel!!
        public MachStatus MachineStatus { get; private set; }
        public RespStatus ResponseStatus { get; private set; }
        public SolidColorBrush MachineStatusColor { get; private set; }
        public GrblModel GrblM { get; private set; }
        public List<GrblModel> ListGrblSettingModel { get; private set; } = new List<GrblModel>();
        public string GrblData { get; private set; } = string.Empty;
        #endregion

        #region Constructor
        public GrblTool()
        {

        }

        public GrblTool(string grblData)
        {
            GrblData = grblData;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sorts Grbl data received like Grbl informations, response, coordinates, settings...
        /// </summary>
        /// <param name="line"></param>
        public void DataGrblSorter(string _line)
        {
            try
            {
                string line = gcodeTool.TrimGcode(_line);
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
                    else if (line.StartsWith("$") && line.Contains("="))
                    {
                        ProcessGrblSettingResponse(_line);
                    }
                    //else if (line.StartsWith("[") || line.EndsWith("]"))
                    else if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        ProcessInfoResponse(line);
                    }
                    else if (line.Contains("rbl"))
                    {
                        InfoMessage = "View startup blocks"; //Grbl 0.9i ['$' for help] put something like $N0=G20 G54 G17 to get it
                    }
                    else
                    {
                        InfoMessage = "Unknown";//TODO
                    }
                }
                //logger.Info("GrblTool|DataGrblSorter|Data:{0}|RespStatus:{1}|MachStatus:{2}", line, ResponseStatus.ToString(), MachineStatus.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("GrblTool|Exception DataGrblSorter raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Processes Grbl build informations.
        /// </summary>
        /// <param name="data"></param>
        public void ProcessInfoResponse(string data)
        {
            try
            {
                ResponseStatus = RespStatus.Ok;
                logger.Info("GrblTool|ProcessInfoResponse|Data:{0}", data);
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

            catch (Exception ex)
            {
                logger.Error("GrblTool|Exception ProcessInfoResponse raised: " + ex.ToString());
            }
        }

        /// <summary>
        /// Processes the serial port ok message reply.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_isError"></param>
        /// <returns></returns>
        public void ProcessResponse(string data)
        {
            ResponseStatus = RespStatus.Ok;
            CanSend=true;
            logger.Info("GrblTool|ProcessResponse|Data:{0}", data);
        }

        /// <summary>
        /// Processes the serial port error message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void ProcessErrorResponse(string data)
        {
            try
            {
                ErrorCodes ec = new ErrorCodes();
                if (VersionGrbl.StartsWith("1"))//In version 1.1 all error codes have ID
                {
                    ErrorMessage = ec.ErrorDict11[data.Split(':')[1]];
                    logger.Info("GrblTool|ProcessErrorResponse|Error key:{0} | description:{1}", data.Split(':')[1], ErrorMessage);
                }
                else
                {
                    if (data.Contains("ID"))//In version 0.9 only error code from 23 to 37 have ID
                    {
                        ErrorMessage = ec.ErrorDict09[data.Split(':')[2]];
                        logger.Info("GrblTool|ProcessErrorResponse|Error key {0} | description:{1}", data.Split(':')[2], ErrorMessage);
                    }
                    else//Error codes w/o ID
                    {
                        ErrorMessage = ec.ErrorDict09[data.Split(':')[1]];
                        logger.Info("GrblTool|ProcessErrorResponse|Error key {0} | description:{1}", data.Split(':')[1], ErrorMessage);
                    }
                }
                ResponseStatus = RespStatus.Ok;//It is an error but still ok to send next command + try/catch
                CanSend = true;
            }
            catch (Exception ex)
            {
                logger.Error("GrblTool|Exception ProcessErrorResponse raised: ", ex.ToString());
                ResponseStatus = RespStatus.NOk;
            }
        }

        /// <summary>
        /// Processes the serial port alarm message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void ProcessAlarmResponse(string data)
        {
            ResponseStatus = RespStatus.NOk;
            MachineStatusColor = Brushes.Red;
            MachineStatus = MachStatus.Alarm;
            CanSend = false;
            logger.Info("GrblTool|ProcessResponse|Data:{0}|RespStatus:{1}|MachStatus:{2}", data, ResponseStatus.ToString(), MachineStatus.ToString());
            AlarmCodes ac = new AlarmCodes();
            try
            {
                if (VersionGrbl.StartsWith("1"))
                {
                    AlarmMessage = ac.AlarmDict11[data.Split(':')[1]];
                    logger.Info("GrblTool|ProcessAlarmResponse11|Alarm key {0} | description:{1}", data.Split(':')[1], AlarmMessage);
                }
                else
                {
                    AlarmMessage = ac.AlarmDict09[data.Split(':')[1]];
                    logger.Info("GrblTool|ProcessAlarmResponse09|Alarm key {0} | description:{1}", data.Split(':')[1], AlarmMessage);

                }
            }
            catch (Exception ex)
            {
                logger.Error("GrblTool|Exception ProcessAlarmResponse raised: ", ex.ToString());
                ResponseStatus = RespStatus.NOk;
            }
        }

        /// <summary>
        /// Populates the settingsCollection w/ data received w/ Grbl '$$' command.
        /// </summary>
        /// <param name="data"></param>
        public void ProcessGrblSettingResponse(string data)
        {
            try
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
                            GrblM = new GrblModel(arr[0], arr[1], arr[2]);
                            ListGrblSettingModel.Add(GrblM);
                            logger.Info("GrblTool|ProcessGrblSettingResponse|Grbl0.9 settings: {0}|{1}|{2}", arr[0], arr[1], arr[2]);
                        }
                        else//Grbl version 1.1 (w/o setting description)
                        {
                            GrblM = new GrblModel(arr[0], arr[1], sc.SettingDict[arr[0]]);
                            ListGrblSettingModel.Add(GrblM);
                            logger.Info("GrblTool|ProcessGrblSettingResponse|Grbl1.1 settings: {0}|{1}|{2}", arr[0], arr[1], arr[2]);
                        }
                    }
                }
                else
                {
                    logger.Info("GrblTool|ProcessGrblSettingResponse|Unknown");
                    ResponseStatus = RespStatus.NOk;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GrblTool|Exception ProcessGrblSettingResponse raised: ", ex.ToString());
                ResponseStatus = RespStatus.NOk;
            }
        }

        /// <summary>
        /// Get coordinates and status depending of Grbl version 0.9 or 1.1.
        /// </summary>
        /// <param name="data"></param>
        public void ProcessCurrentStatusResponse(string data)
        {
            try
            {
                ResponseStatus = RespStatus.Ok;
                if (data.Contains("|") || VersionGrbl.StartsWith("1"))//Report state Grbl v1.1 < Idle|MPos:0.000,0.000,0.000>
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

                    if(!data.Contains("bf"))
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
                    }
                }
                else//Report state Grbl v0.9 <Idle,MPos:0.000,0.000,0.000,WPos:0.000,0.000,0.000,Buf:0,RX:0>
                {
                    string[] arr = data.Split(new Char[] { '<', '>', ',', ':', '\r', '\n' });
                    if (arr.Length>0)
                    {
                        if (arr.Length > 3)
                        {
                            MachinePositionX = arr[3];
                            MachinePositionY = arr[4];
                        }
                        if (arr.Length > 7)
                        {
                            WorkPositionX = arr[7];
                            WorkPositionY = arr[8];
                        }
                        if (arr.Length > 11)
                        {
                            PlannerBuffer = arr[11];
                        }
                        if (arr.Length > 13)
                        {
                            RxBuffer = arr[13];
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
                        }
                    }
                    else
                    {
                        logger.Info("GrblTool|ProcessCurrentStatusResponse|Unknown");
                        ResponseStatus = RespStatus.NOk;
                    }
                }
                //logger.Info("GrblTool|ProcessCurrentStatusResponse|Current state:{0}|RespStatus:{1}|MachStatus:{2}|Color:{3}", _data, ResponseStatus.ToString(), MachineStatus.ToString(), MachineStatusColor.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("GrblTool|Exception ProcessCurrentStatusResponse raised: ", ex.ToString());
                ResponseStatus = RespStatus.NOk;
            }
        }
        #endregion
    }
}
