using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Windows.Media;
using VDLaser.Core.Codes;
using VDLaser.Core.Models;

namespace VDLaser.Core.Tools
{
    /// <summary>
    /// Tools to parse grbl data send or received
    /// </summary>
    public class GrblParser
    {
        #region Fields
        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly GrblSettingCodes _grblSettingCode = new();
        private readonly ErrorCodes _grblErrorCode = new();
        private readonly AlarmCodes _grblAlarmCode = new();

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
        public double MachinePositionX { get; private set; } = 0;
        public double MachinePositionY { get; private set; } = 0;
        public double WorkPositionX { get; private set; } = 0;
        public double WorkPositionY { get; private set; } = 0;
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
        public MachStatus MachineStatus { get; private set; } = MachStatus.Undefined;
        public RespStatus ResponseStatus { get; private set; } = RespStatus.Ok;
        public SolidColorBrush MachineStatusColor { get; private set; } = Brushes.DarkGray;
        public SettingItems GrblSettings { get; private set; } = new SettingItems();
        public List<SettingItems> ListGrblSettings { get; private set; } = new List<SettingItems>();
        #endregion

        #region Constructor
        public GrblParser() { }
        #endregion

        #region Methods
        /// <summary>
        /// Sorts GRBL data received like GRBL informations, response, coordinates, settings...
        /// </summary>
        /// <param name="line">The raw line from GRBL.</param>
        /// <param name="isVersion11">True for GRBL 1.1, false for 0.9.</param>
        public void DataGrblSorter(string? line, bool isVersion11)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            var lineTrim = GCodeTool.TrimGcode(line) ?? string.Empty;
            lineTrim = lineTrim.ToLowerInvariant();  // Normalisation pour case-insensitive

            switch (lineTrim)
            {
                case var _ when lineTrim.StartsWith("ok"):
                    ProcessResponse(lineTrim);
                    break;
                case var _ when lineTrim.StartsWith("error"):
                    ProcessErrorResponse(lineTrim, isVersion11);
                    break;
                case var _ when lineTrim.StartsWith("alarm"):
                    ProcessAlarmResponse(lineTrim, isVersion11);
                    break;
                case var _ when lineTrim.StartsWith("<") && lineTrim.EndsWith(">"):
                    ProcessStatusResponse(lineTrim, isVersion11);
                    break;
                case var _ when lineTrim.StartsWith("$") && lineTrim.Contains("="):
                    if (lineTrim.StartsWith("$n"))
                        ProcessStartupBlockResponse(lineTrim);
                    else
                        ProcessGrblSettingResponse(lineTrim, isVersion11);
                    break;
                case var _ when lineTrim.StartsWith("[") && lineTrim.EndsWith("]"):
                    ProcessInfoResponse(lineTrim);
                    break;
                case var _ when lineTrim.Contains("rbl"):
                    ProcessResetResponse(lineTrim);
                    break;
                default:
                    ProcessInfoResponse(lineTrim);
                    break;
            }
        }

        /// <summary>
        /// Get Grbl reset response
        /// </summary>
        /// <param name="data"></param>
        private void ProcessResetResponse(string data)
        {
            //logger.Info(CultureInfo.CurrentCulture,"GrblTool|ProcessResetResponse: {0}", data);
        }

        /// <summary>
        /// Process Grbl build informations.
        /// </summary>
        /// <param name="data"></param>
        private void ProcessInfoResponse(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            ResponseStatus = RespStatus.Ok;
            InfoMessage = data.Length switch
            {
                9 or 10 => "Check gcode mode",
                16 or 20 when data.Contains("ver") => ParseVersionAndBuild(data),
                19 => "Kill alarm lock or reset to continue?",
                21 => "Kill alarm lock or homing to continue",
                23 => "View G-code parameters",
                44 => "View gcode parser state",
                _ => "Unknown[]"
            };
        }

        /// <summary>
        /// Process the serial port ok message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void ProcessResponse(string data)
        {
            ResponseStatus = RespStatus.Ok;
            //logger.Info(CultureInfo.CurrentCulture, "GrblTool|ProcessResponse|Data:{0}", data);
            AlarmMessage = string.Empty;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Process the serial port error message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void ProcessErrorResponse(string data, bool isVersion11)
        {
            ResponseStatus = RespStatus.Ok;  // Erreur mais OK pour next command
            if (string.IsNullOrEmpty(data)) return;

            var parts = data.Split(':');
            if (isVersion11 && parts.Length > 1 && _grblErrorCode.ErrorDict11.TryGetValue(parts[1], out var msg))
            {
                ErrorMessage = msg;
            }
            else if (!isVersion11 && parts.Length > 1)
            {
                var key = parts.Length > 2 && data.Contains("ID") ? parts[2] : parts[1];
                if (_grblErrorCode.ErrorDict09.TryGetValue(key, out msg))
                    ErrorMessage = msg;
            }
        }

        /// <summary>
        /// Process the serial port alarm message reply.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void ProcessAlarmResponse(string data, bool isVersion11)
        {
            ResponseStatus = RespStatus.NOk;
            MachineStatusColor = Brushes.Red;
            MachineStatus = MachStatus.Alarm;
            if (string.IsNullOrEmpty(data)) return;

            var parts = data.Split(':');
            if (parts.Length > 1)
            {
                var dict = isVersion11 ? _grblAlarmCode.AlarmDict11 : _grblAlarmCode.AlarmDict09;
                if (dict.TryGetValue(parts[1], out var msg))
                    AlarmMessage = msg;
            }
        }

        /// <summary>
        /// Populate the settingsCollection w/ data received w/ Grbl '$$' command.
        /// </summary>
        /// <param name="data"></param>
        private void ProcessGrblSettingResponse(string data, bool isVersion11)
        {
            if (string.IsNullOrEmpty(data)) return;

            ResponseStatus = RespStatus.Q;  // Wait for all settings
            var parts = data.Split(new[] { '=', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return;

            if (data.Contains("N"))
            {
                InfoMessage = "Startup block";
            }
            else
            {
                var description = parts.Length > 2 ? parts[2] : (isVersion11 ? _grblSettingCode.SettingDict.GetValueOrDefault(parts[0], "Unknown") : parts[2]);
                GrblSettings = new SettingItems(parts[0], parts[1], description);
                ListGrblSettings.Add(GrblSettings);
            }
        }

        /// <summary>
        /// Get coordinates and status depending of Grbl version 0.9 or 1.1.
        /// Grbl v0.9 <Idle,MPos:0.000,0.000,0.000,WPos:0.000,0.000,0.000,Buf:0,RX:0>
        /// Grbl v1.1 < Idle|MPos:0.000,0.000,0.000>
        /// WorkPositionX = MachinePositionX - OffsetPositionX
        /// </summary>
        /// <param name="data"></param>
        public void ProcessStatusResponse(string data, bool isVersion11)
        {
            if (string.IsNullOrEmpty(data)) return;

            var parts = data.Split(new[] { '<', '>', ',', ':', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return;

            ResponseStatus = RespStatus.Ok;
            MachineStatus = Enum.TryParse<MachStatus>(parts[0], true, out var status) ? status : MachStatus.Undefined;
            MachineStatusColor = MachineStatus switch
            {
                MachStatus.Idle => Brushes.Beige,
                MachStatus.Run => Brushes.LightGreen,
                MachStatus.Hold => Brushes.LightBlue,
                MachStatus.Alarm => Brushes.Red,
                MachStatus.Door => Brushes.LightYellow,
                MachStatus.Check => Brushes.LightCyan,
                MachStatus.Home => Brushes.LightPink,
                _ => Brushes.DarkGray
            };

            // Parse positions, buffers, etc. avec try-parse pour robustesse
            if (isVersion11)
            {
                // Logique pour 1.1 simplifiée
                if (data.Contains("mpos"))
                {
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var mx);
                    double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var my);
                    MachinePositionX = mx;
                    MachinePositionY = my;
                    WorkPositionX = 0;
                    WorkPositionY = 0;
                }
                else
                {
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var wx);
                    double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var wy);
                    WorkPositionX = wx;
                    WorkPositionY = wy;
                    MachinePositionX = 0;
                    MachinePositionY = 0;
                }
                if (data.Contains("bf"))
                {
                    double.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var pb);
                    double.TryParse(parts[7], NumberStyles.Any, CultureInfo.InvariantCulture, out var rb);
                    double.TryParse(parts[9], NumberStyles.Any, CultureInfo.InvariantCulture, out var mf);
                    double.TryParse(parts[10], NumberStyles.Any, CultureInfo.InvariantCulture, out var ms);
                    PlannerBuffer = pb.ToString(CultureInfo.InvariantCulture);
                    RxBuffer = rb.ToString(CultureInfo.InvariantCulture);
                    MachineFeed = mf.ToString(CultureInfo.InvariantCulture);
                    MachineSpeed = ms.ToString(CultureInfo.InvariantCulture);
                    if (data.Contains("wco"))
                    {
                        double.TryParse(parts[12], NumberStyles.Any, CultureInfo.InvariantCulture, out var opx);
                        double.TryParse(parts[13], NumberStyles.Any, CultureInfo.InvariantCulture, out var opy);
                        OffsetPositionX = opx.ToString(CultureInfo.InvariantCulture);
                        OffsetPositionY = opy.ToString(CultureInfo.InvariantCulture);
                    }
                    if (data.Contains("Ov"))
                    {
                        double.TryParse(parts[12], NumberStyles.Any, CultureInfo.InvariantCulture, out var omf);
                        double.TryParse(parts[13], NumberStyles.Any, CultureInfo.InvariantCulture, out var oms);
                        OverrideMachineFeed = omf.ToString(CultureInfo.InvariantCulture);
                        OverrideMachineSpeed = oms.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    double.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var mf);
                    double.TryParse(parts[7], NumberStyles.Any, CultureInfo.InvariantCulture, out var ms);
                    MachineFeed = mf.ToString(CultureInfo.InvariantCulture);
                    MachineSpeed = ms.ToString(CultureInfo.InvariantCulture);
                    PlannerBuffer = "0";
                    RxBuffer = "0";
                    if (data.Contains("wco"))
                    {
                        double.TryParse(parts[9], NumberStyles.Any, CultureInfo.InvariantCulture, out var opx);
                        double.TryParse(parts[10], NumberStyles.Any, CultureInfo.InvariantCulture, out var opy);
                        OffsetPositionX = opx.ToString(CultureInfo.InvariantCulture);
                        OffsetPositionY = opy.ToString(CultureInfo.InvariantCulture);
                    }
                    if (data.Contains("Ov"))
                    {
                        double.TryParse(parts[9], NumberStyles.Any, CultureInfo.InvariantCulture, out var omf);
                        double.TryParse(parts[10], NumberStyles.Any, CultureInfo.InvariantCulture, out var oms);
                        OverrideMachineFeed = omf.ToString(CultureInfo.InvariantCulture);
                        OverrideMachineSpeed = oms.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            else//v0.9 à revoir déjà repris plus haut
            {
                if (data.Length > 13)
                {
                    RxBuffer = parts[13];
                }
                if (data.Length > 11)
                {
                    PlannerBuffer = parts[11];
                }

                if (data.Length > 7)
                {
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var wx);
                    double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var wy);
                    WorkPositionX = wx;
                    WorkPositionY = wy;
                }
                if (data.Length > 3)
                {
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var mx);
                    double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var my);
                    MachinePositionX = mx;
                    MachinePositionY = my;
                }
            }
        }

        /// <summary>
        /// Get Grbl startup block message
        /// </summary>
        /// <param name="data"></param>
        public void ProcessStartupBlockResponse(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            ResponseStatus = RespStatus.Ok;
            InfoMessage = "View startup blocks";
            //logger.Info(CultureInfo.CurrentCulture, "GrblTool|ProcessStartupBlockResponse|Data:{0}", data);
        }

        /// <summary>
        /// Extraire la version et build de Grbl                
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string ParseVersionAndBuild(string data)
        {
            var parts = data.Split(new[] { '[', ']', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                    VersionGrbl = parts[1].Length >= 4 ? parts[1].Substring(0,4): "-";
                    BuildInfo = parts[1].Length >= 8 ? parts[1].Substring(5,8) : "-";
            }
            return string.Empty;  // Retourne vide car InfoMessage est set ailleurs si needed
        }
        public GrblStatus Parse(string line)
        {
            throw new NotImplementedException();
        }

        //Ajouter méthodes comme ProcessAlarm, ProcessError, avec try-parse pour robustesse)
        #endregion
    }

    public class GrblStatus
    {
    }
}
