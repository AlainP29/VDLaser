using System.Globalization;
using System.Windows.Media;
using VDLaser.Core.Codes;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Core.Grbl.Parsers
{
    public class GrblStateParser : IGrblSubParser
    {
        private readonly ILogService _log;

        public string Name => "GrblStatusParser";

        private readonly AlarmCodes _alarms = new();
        /// <summary>
        /// Enumeration of the response states. Ok: All is good, NOk: Alarm state Q: Queued [DR: Data received] 
        /// </summary>
        public enum RespStatus { Ok, NOk, Q };

        public MachState MachineState { get; private set; } = MachState.Undefined;
        public SolidColorBrush MachineStateColor { get; private set; } = Brushes.DarkGray;
        public GrblStateParser()
        {
        }
        public GrblStateParser(ILogService log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }
        public bool CanParse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            line = line.Trim().ToLowerInvariant();

            return line.StartsWith("<") && line.EndsWith(">");
        }

        public void Parse(string line, GrblState state)
        {
            if (string.IsNullOrWhiteSpace(line) || state == null)
                return;
            try
            {
                var content = line.Trim('<', '>');
                var parts = content.Split('|', StringSplitOptions.RemoveEmptyEntries);
                _log.Debug("[GrblStateParser Parser Start] Parsing status content: {Content}", content);
                if (parts.Length == 0)
                    return;

                ParseMachineState(parts[0], state);

                for (int i = 1; i < parts.Length; i++)
                {
                    ParseBlock(parts[i], state);
                    if (parts[i].StartsWith("FS:"))
                    {
                        _log.Debug("[GrblStateParser Parser FS] Block: {Block}, Extracting S...", parts[i]); 
                    }
                }
                _log.Debug("[GrblStateParser Parser End] Parsed state - PowerLaser: {PowerLaser}, MachineState: {State}", state.PowerLaser, state.MachineState);
                UpdateStatusColor(state);
            }

            catch (Exception ex)
            {
                state.ErrorMessage = ex.Message;
                _log.Error("[GrblStateParser] Parse error: {Msg}", ex.Message);
            }
        }

            // -----------------------
            // MACHINE STATE
            // -----------------------
            private static void ParseMachineState(string token, GrblState state)
        {
            var stateName = token.Split(':')[0];
            if (Enum.TryParse<MachState>(stateName, true, out var ms))
                state.MachineState = ms;
            else
                state.MachineState = MachState.Undefined;
        }
        // =====================================================
        // BLOCK ROUTING
        // =====================================================
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
                var (x, y, z) = ParseVector(block, "WPos:");
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

        // =====================================================
        // HELPERS
        // =====================================================
        private static (double x, double y, double z) ParseVector(string block, string prefix)
        {
            var values = block[prefix.Length..].Split(',');

            double.TryParse(values.ElementAtOrDefault(0), NumberStyles.Float, CultureInfo.InvariantCulture, out var x);
            double.TryParse(values.ElementAtOrDefault(1), NumberStyles.Float, CultureInfo.InvariantCulture, out var y);
            double.TryParse(values.ElementAtOrDefault(2), NumberStyles.Float, CultureInfo.InvariantCulture, out var z);

            return (x, y, z);
        }
        // =====================================================
        // UI COLOR
        // =====================================================
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
        // =====================================================
        // UNUSED
        // =====================================================
        public void Parse(string line, GrblInfo state)
        {
        }
        private static bool TryParseTriple(string token, string key,
    out double a, out double b, out double c)
        {
            a = b = c = 0;

            if (!token.StartsWith(key + ":", StringComparison.OrdinalIgnoreCase))
                return false;

            var parts = token[(key.Length + 1)..].Split(',');
            return parts.Length >= 3
                && double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out a)
                && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out b)
                && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out c);
        }

        private static bool TryParsePair(string token, string key,
            out double a, out double b)
        {
            a = b = 0;

            if (!token.StartsWith(key + ":", StringComparison.OrdinalIgnoreCase))
                return false;

            var parts = token[(key.Length + 1)..].Split(',');
            return parts.Length >= 2
                && double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out a)
                && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out b);
        }

    }
}

