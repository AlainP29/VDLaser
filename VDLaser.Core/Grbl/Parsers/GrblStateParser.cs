using System.Globalization;
using System.Windows.Media;
using VDLaser.Core.Codes;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Core.Grbl.Parsers
{
    /// <summary>
    /// Parses GRBL status reports such as:
    /// <Idle|MPos:0.000,0.000,0.000|FS:0,0>
    /// Updates the GrblState object with machine position, feed, speed, overrides, etc.
    /// </summary>
    public class GrblStateParser : IGrblSubParser
    {
        private readonly ILogService? _log;

        public string Name => "GrblStatusParser";

        private readonly AlarmCodes _alarms = new();

        public MachState MachineState { get; private set; } = MachState.Undefined;
        public SolidColorBrush MachineStateColor { get; private set; } = Brushes.DarkGray;

        #region Constructors

        public GrblStateParser() { }

        public GrblStateParser(ILogService log)
        {
            _log = log;
            _log?.Information("[GrblStateParser] Initialised");
        }

        #endregion

        #region CanParse

        /// <summary>
        /// Determines whether the line is a GRBL status report.
        /// Status reports always start with '<' and end with '>'.
        /// </summary>
        public bool CanParse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            line = line.Trim().ToLowerInvariant();
            return line.StartsWith("<") && line.EndsWith(">");
        }

        #endregion

        #region Parse Entry Point

        /// <summary>
        /// Parses a full GRBL status report and updates the GrblState object.
        /// </summary>
        public void Parse(string line, GrblState state)
        {
            if (string.IsNullOrWhiteSpace(line) || state == null)
                return;

            try
            {
                var content = line.Trim('<', '>');
                var parts = content.Split('|', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    return;

                _log?.Debug("[GrblStateParser] Parsing status: {Content}", content);

                // First token = machine state (Idle, Run, Hold, Alarm, etc.)
                ParseMachineState(parts[0], state);

                // Remaining tokens = data blocks
                for (int i = 1; i < parts.Length; i++)
                {
                    ParseBlock(parts[i], state);
                }

                UpdateStatusColor(state);

                _log?.Debug("[GrblStateParser] State updated: {State}", state.MachineState);
            }
            catch (Exception ex)
            {
                state.ErrorMessage = ex.Message;
                _log?.Error("[ConsoleViewModel] Failed to export logs to file:", ex);
            }
        }

        #endregion

        #region Machine State

        /// <summary>
        /// Parses the machine state token (Idle, Run, Hold, Alarm, etc.).
        /// </summary>
        private static void ParseMachineState(string token, GrblState state)
        {
            var stateName = token.Split(':')[0];

            if (Enum.TryParse<MachState>(stateName, true, out var ms))
                state.MachineState = ms;
            else
                state.MachineState = MachState.Undefined;
        }

        #endregion

        #region Block Parsing

        /// <summary>
        /// Routes each block to the appropriate parser.
        /// </summary>
        private static void ParseBlock(string block, GrblState state)
        {
            if (block.StartsWith("MPos:"))
            {
                var (x, y, z) = ParseVector(block, "MPos:");
                state.MachinePosX = x;
                state.MachinePosY = y;
                state.MachinePosZ = z;
            }
            else if (block.StartsWith("WPos:"))
            {
                var (x, y, z) = ParseVector(block, "WWPos:");
                state.WorkPosX = x;
                state.WorkPosY = y;
                state.WorkPosZ = z;
            }
            else if (block.StartsWith("WCO:"))
            {
                var (x, y, z) = ParseVector(block, "WCO:");
                state.OffsetPosX = x.ToString(CultureInfo.InvariantCulture);
                state.OffsetPosY = y.ToString(CultureInfo.InvariantCulture);
                state.OffsetPosZ = z.ToString(CultureInfo.InvariantCulture);
            }
            else if (block.StartsWith("FS:"))
            {
                var values = block["FS:".Length..].Split(',');
                if (values.Length >= 2)
                {
                    state.MachineFeed = values[0];
                    state.MachineSpeed = values[1];
                    state.PowerLaser = values[1];
                }
            }
            else if (block.StartsWith("Ov:"))
            {
                var values = block["Ov:".Length..].Split(',');
                if (values.Length >= 2)
                {
                    state.OverrideMachineFeed = values[0];
                    state.OverrideMachineSpeed = values[1];
                }
            }
            else if (block.StartsWith("Bf:"))
            {
                var values = block["Bf:".Length..].Split(',');
                if (values.Length >= 2)
                {
                    int.TryParse(values[0], out int bf);
                    int.TryParse(values[1], out int rx);
                    state.PlannerBuffer = bf;
                    state.RxBuffer = rx;
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Parses a generic X,Y,Z vector from a block.
        /// </summary>
        private static (double x, double y, double z) ParseVector(string block, string prefix)
        {
            var values = block[prefix.Length..].Split(',');

            double.TryParse(values.ElementAtOrDefault(0), NumberStyles.Float, CultureInfo.InvariantCulture, out var x);
            double.TryParse(values.ElementAtOrDefault(1), NumberStyles.Float, CultureInfo.InvariantCulture, out var y);
            double.TryParse(values.ElementAtOrDefault(2), NumberStyles.Float, CultureInfo.InvariantCulture, out var z);

            return (x, y, z);
        }

        /// <summary>
        /// Updates the UI color associated with the machine state.
        /// </summary>
        private static void UpdateStatusColor(GrblState state)
        {
            state.MachineStatusColor = state.MachineState switch
            {
                MachState.Idle => Brushes.Beige,
                MachState.Run => Brushes.LightGreen,
                MachState.Hold => Brushes.LightBlue,
                MachState.Alarm => Brushes.Red,
                MachState.Door => Brushes.Orange,
                MachState.Home => Brushes.LightPink,
                _ => Brushes.DarkGray
            };
        }

        #endregion

        #region Unused Interface Methods

        public void Parse(string line, GrblInfo state) { }

        #endregion
    }
}
