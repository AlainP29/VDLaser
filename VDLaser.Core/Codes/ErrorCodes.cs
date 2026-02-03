using VDLaser.Core.Grbl.Models;

namespace VDLaser.Core.Codes
{
    /// <summary>
    /// Error codes for GRBL 0.9 and 1.1
    /// </summary>
    public class ErrorCodes
    {
        /// <summary>
        /// GRBL 0.9 error dictionary.
        /// Key = error code, Value = GrblError
        /// </summary>
        public Dictionary<int, GrblError> ErrorDict09 { get; } = new()
        {
            { 1, new GrblError(1, "Letter missing", "G-code word letter is missing (ex: 'X', 'G', 'M').") },
            { 2, new GrblError(2, "Bad number format", "Numeric value missing or invalid.") },
            { 3, new GrblError(3, "Invalid statement", "Invalid or unknown $ system command.") },
            { 4, new GrblError(4, "Negative value", "A value that must be positive is negative.") },
            { 5, new GrblError(5, "Homing not enabled", "Homing required but not enabled.") },
            { 6, new GrblError(6, "Pulse < 3µs", "Step pulse time too short (<3µs).") },
            { 7, new GrblError(7, "EEPROM read fail", "EEPROM read error — defaults restored.") },
            { 8, new GrblError(8, "Not idle", "Command blocked because machine is not IDLE.") },
            { 9, new GrblError(9, "G-code locked", "Command blocked because machine is in alarm.") },

            { 10, new GrblError(10, "Soft limits error", "Soft limits require homing to be enabled.") },
            { 11, new GrblError(11, "Line overflow", "Too many characters in G-code line (>80).") },
            { 21, new GrblError(21, "Modal group violation", "Two G-codes of same modal group in one line.") },
            { 22, new GrblError(22, "Undefined feed rate", "Feed rate has not been set.") },
            { 23, new GrblError(23, "Integer expected", "Integer value expected for G or M command.") },
            { 24, new GrblError(24, "Axis word conflict", "Two commands require XYZ words in same block.") },
            { 25, new GrblError(25, "Word repeated", "A G-code word was repeated in the block.") },
            { 26, new GrblError(26, "Missing axis words", "Command requires XYZ axis words but none found.") },
            { 27, new GrblError(27, "Line number range", "Line number exceeds maximum allowed.") },
            { 28, new GrblError(28, "Missing P or L value", "Command missing mandatory P or L word.") },
            { 29, new GrblError(29, "Unsupported WCS", "Work coordinate system not supported (G59.x).") },
            { 30, new GrblError(30, "G53 requires G0/G1", "G53 requires active G0/G1 motion mode.") },
            { 31, new GrblError(31, "Unused words", "Unused axis words detected in G80 mode.") },
            { 32, new GrblError(32, "Arc axis missing", "Arc command missing required axis words.") },
            { 33, new GrblError(33, "Invalid target", "Arc/probe target is invalid.") },
            { 34, new GrblError(34, "Bad arc radius", "Arc radius mathematically invalid.") },
            { 35, new GrblError(35, "Arc offset missing", "Arc command missing IJK offset.") },
            { 36, new GrblError(36, "Leftover words", "Unused G-code words in the block.") },
            { 37, new GrblError(37, "Tool length axis", "Tool length offset applies only to Z axis.") }
        };

        /// <summary>
        /// GRBL 1.1 error dictionary.
        /// Key = error code, Value = GrblError
        /// </summary>
        public Dictionary<int, GrblError> ErrorDict11 { get; } = new()
        {
            { 1, new GrblError(1, "Letter missing", "G-code word letter is missing.") },
            { 2, new GrblError(2, "Bad number format", "Invalid or missing numeric value.") },
            { 3, new GrblError(3, "Invalid statement", "Unknown or invalid '$' command.") },
            { 4, new GrblError(4, "Negative value", "Negative value where positive is required.") },
            { 5, new GrblError(5, "Homing disabled", "Homing must be enabled.") },
            { 6, new GrblError(6, "Pulse < 3µs", "Minimum step pulse is < 3 microseconds.") },
            { 7, new GrblError(7, "EEPROM read fail", "EEPROM read failed — defaults restored.") },
            { 8, new GrblError(8, "Not idle", "Command blocked unless machine is IDLE.") },
            { 9, new GrblError(9, "G-code locked", "Command blocked during alarm or jog state.") },

            { 10, new GrblError(10, "Soft limits", "Soft limits require homing enabled.") },
            { 11, new GrblError(11, "Line overflow", "Max characters per line exceeded.") },
            { 12, new GrblError(12, "Max step rate", "Configured max step rate exceeded.") },
            { 13, new GrblError(13, "Safety door", "Safety door triggered.") },
            { 14, new GrblError(14, "EEPROM length", "EEPROM line length exceeded.") },
            { 15, new GrblError(15, "Jog travel", "Jog exceeds travel limits.") },
            { 16, new GrblError(16, "Jog format", "Malformed jog command.") },
            { 17, new GrblError(17, "Laser PWM", "Laser mode requires PWM output.") },

            { 20, new GrblError(20, "Unsupported command", "Unsupported or invalid G-code command.") },
            { 21, new GrblError(21, "Modal group violation", "Two G-codes from same modal group detected.") },
            { 22, new GrblError(22, "Undefined feed rate", "Feed rate not defined.") },
            { 23, new GrblError(23, "Integer expected", "Integer value expected for command.") },
            { 24, new GrblError(24, "Axis conflict", "Two commands require XYZ words.") },
            { 25, new GrblError(25, "Word repeated", "G-code word repeated.") },
            { 26, new GrblError(26, "Missing axis words", "Command requires axis words but none found.") },
            { 27, new GrblError(27, "Line number range", "Line number outside allowed range.") },
            { 28, new GrblError(28, "Missing P or L", "Missing required P or L word.") },
            { 29, new GrblError(29, "Unsupported WCS", "Unsupported work coordinate system.") },
            { 30, new GrblError(30, "G53 requires G0/G1", "G53 requires G0/G1 motion mode.") },
            { 31, new GrblError(31, "Unused words", "Unused axis words in the block.") },
            { 32, new GrblError(32, "Arc axis missing", "No axis words for arc command.") },
            { 33, new GrblError(33, "Invalid target", "Arc/probe target invalid.") },
            { 34, new GrblError(34, "Bad arc radius", "Arc radius error.") },
            { 35, new GrblError(35, "Arc offset missing", "Missing IJK arc offsets.") },
            { 36, new GrblError(36, "Leftover words", "Unused leftover G-code words.") },
            { 37, new GrblError(37, "Tool length axis", "Dynamic tool offset applies only to Z.") },
            { 38, new GrblError(38, "Tool number too large", "Tool number exceeds max.") }
        };

        /// <summary>
        /// Retrieve an error by code and version.
        /// </summary>
        public GrblError? GetError(int code, bool isVersion11)
        {
            if (isVersion11 && ErrorDict11.TryGetValue(code, out var err11))
                return err11;

            if (!isVersion11 && ErrorDict09.TryGetValue(code, out var err09))
                return err09;

            return null;
        }
    }
}
