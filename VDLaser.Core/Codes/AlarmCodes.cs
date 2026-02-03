using System.Collections.Generic;
using VDLaser.Core.Grbl.Models;

namespace VDLaser.Core.Codes
{
    /// <summary>
    /// Alarm codes for GRBL 0.9 and 1.1
    /// </summary>
    public class AlarmCodes
    {
        /// <summary>
        /// Dictionary for GRBL 1.1 alarm codes.
        /// Key = alarm code, Value = GrblAlarm
        /// </summary>
        public Dictionary<int, GrblAlarm> AlarmDict11 { get; } = new()
        {
            { 1, new GrblAlarm(1, "Hard limit triggered. Machine position is likely lost.", AlarmSeverity.Critical) },
            { 2, new GrblAlarm(2, "Soft limit triggered. G-code motion target is out of bounds.", AlarmSeverity.Critical) },
            { 3, new GrblAlarm(3, "Reset while in motion. Machine position may be lost.", AlarmSeverity.Warning) },
            { 4, new GrblAlarm(4, "Probe fail. Probe is not triggered during probing cycle.", AlarmSeverity.Critical) },
            { 5, new GrblAlarm(5, "Probe fail. Probe is triggered before probing cycle.", AlarmSeverity.Warning) },
            { 6, new GrblAlarm(6, "Homing fail. Safety door was opened.", AlarmSeverity.Warning) },
            { 7, new GrblAlarm(7, "Homing fail. Pull-off failed to clear limit switch.", AlarmSeverity.Critical) },
            { 8, new GrblAlarm(8, "Homing fail. Could not find limit switch.", AlarmSeverity.Critical) },
            { 9, new GrblAlarm(9, "Homing fail. Homing is not enabled via settings.", AlarmSeverity.Warning) }
        };

        /// <summary>
        /// Dictionary for GRBL 0.9 alarm codes.
        /// </summary>
        public Dictionary<int, GrblAlarm> AlarmDict09 { get; } = new()
        {
            { 1, new GrblAlarm(1, "Hard limit triggered.", AlarmSeverity.Critical) },
            { 2, new GrblAlarm(2, "Soft limit triggered.", AlarmSeverity.Critical) },
            { 3, new GrblAlarm(3, "Abort during cycle.", AlarmSeverity.Warning) },
            { 4, new GrblAlarm(4, "Probe fail.", AlarmSeverity.Critical) },
            { 5, new GrblAlarm(5, "Homing fail.", AlarmSeverity.Warning) }
        };
        /// <summary>
        /// Retrieve alarm by code depending on GRBL version.
        /// Returns null if code is unknown.
        /// </summary>
        public GrblAlarm? GetAlarm(int code, bool isVersion11)
        {
            if (isVersion11 && AlarmDict11.TryGetValue(code, out var alarm11))
                return alarm11;

            if (!isVersion11 && AlarmDict09.TryGetValue(code, out var alarm09))
                return alarm09;

            return null;
        }
    }
}