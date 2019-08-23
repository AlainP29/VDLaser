using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGrbl.Codes
{
    /// <summary>
    /// Grbl Error codes.
    /// </summary>
    class ErrorCodes
    {
        /// <summary>
        /// Description of the error messages of Grbl version 0.9 (ErrorDict09) and 1.1 (ErrorDict11) only
        /// </summary>
        public ErrorCodes()
        {
            ErrorDict09.Add(" expected command letter", " G - code is composed of G - code 'words', which consists of a letter followed by a number value.This error occurs when the letter prefix of a G - code word is missing in the G - code block(aka line).");
            ErrorDict09.Add(" bad number format", "The number value suffix of a G-code word is missing in the G-code block, or when configuring a $Nx=line or $x=val Grbl setting and the x is not a number value.");
            ErrorDict09.Add(" invalid statement", " The issued Grbl $ system command is not recognized or is invalid.");
            ErrorDict09.Add(" value < 0", " The value of a $x=val Grbl setting, F feed rate, N line number, P word, T tool number, or S spindle speed is negative.");
            ErrorDict09.Add(" setting disabled", " Homing is disabled when issuing a $H command.");
            ErrorDict09.Add(" value < 3 usec", " Step pulse time length cannot be less than 3 microseconds (for technical reasons).");
            ErrorDict09.Add(" EEPROM read fail. Using defaults", " If Grbl can't read data contained in the EEPROM, this error is returned. Grbl will also clear and restore the effected data back to defaults.");
            ErrorDict09.Add(" not idle", " Certain Grbl $ commands are blocked depending Grbl's current state, or what its doing. In general, Grbl blocks any command that fetches from or writes to the EEPROM since the AVR microcontroller will shutdown all of the interrupts for a few clock cycles when this happens. There is no work around, other than blocking it. This ensures both the serial and step generator interrupts are working smoothly throughout operation.");
            ErrorDict09.Add(" alarm lock", " Grbl enters an ALARM state when Grbl doesn't know where it is and will then block all G-code commands from being executed. This error occurs if G-code commands are sent while in the alarm state. Grbl has two alarm scenarios: When homing is enabled, Grbl automatically goes into an alarm state to remind the user to home before doing anything; When something has went critically wrong, usually when Grbl can't guarantee positioning. This typically happens when something causes Grbl to force an immediate stop while its moving from a hard limit being triggered or a user commands an ill-timed reset.");
            ErrorDict09.Add(" homing not enabled", " Soft limits cannot be enabled if homing is not enabled, because Grbl has no idea where it is when you startup your machine unless you perform a homing cycle.");
            ErrorDict09.Add(" line overflow", " Grbl has to do everything it does within 2KB of RAM. Not much at all. So, we had to make some decisions on what's important. Grbl limits the number of characters in each line to less than 80 characters (70 in v0.8, 50 in v0.7 or earlier), excluding spaces or comments. The G-code standard mandates 256 characters, but Grbl simply doesn't have the RAM to spare. However, we don't think there will be any problems with this with all of the expected G-code commands sent to Grbl. This error almost always occurs when a user or CAM-generated G-code program sends position values that are in double precision (i.e. -2.003928578394852), which is not realistic or physically possible. Users and GUIs need to send Grbl floating point values in single precision (i.e. -2.003929) to avoid this error.");
            ErrorDict09.Add(" modal group violation", " The G-code parser has detected two G-code commands that belong to the same modal group in the block/line. Modal groups are sets of G-code commands that mutually exclusive. For example, you can't issue both a G0 rapids and G2 arc in the same line, since they both need to use the XYZ target position values in the line. LinuxCNC.org has some great documentation on modal groups.");
            ErrorDict09.Add(" unsupported command", " The G-code parser doesn't recognize or support one of the G-code commands in the line. Check your G-code program for any unsupported commands and either remove them or update them to be compatible with Grbl.");
            ErrorDict09.Add(" undefined feed rate", " There is no feed rate programmed, and a G-code command that requires one is in the block/line. The G-code standard mandates F feed rates to be undefined upon a reset or when switching from inverse time mode to units mode. Older Grbl versions had a default feed rate setting, which was illegal and was removed in Grbl v0.9.");
            ErrorDict09.Add("23", "	A G or M command value in the block is not an integer. For example, G4 can't be G4.13. Some G-code commands are floating point (G92.1), but these are ignored.");
            ErrorDict09.Add("24", "	Two G-code commands that both require the use of the XYZ axis words were detected in the block.");
            ErrorDict09.Add("25", "A G-code word was repeated in the block.");
            ErrorDict09.Add("26", "	A G-code command implicitly or explicitly requires XYZ axis words in the block, but none were detected.");
            ErrorDict09.Add("27", "	The G-code protocol mandates N line numbers to be within the range of 1-99,999. We think that's a bit silly and arbitrary. So, we increased the max number to 9,999,999. This error occurs when you send a number more than this.");
            ErrorDict09.Add("28", "	A G-code command was sent, but is missing some important P or L value words in the line. Without them, the command can't be executed. Check your G-code.");
            ErrorDict09.Add("29", "	Grbl supports six work coordinate systems G54-G59. This error happens when trying to use or configure an unsupported work coordinate system, such as G59.1, G59.2, and G59.3.");
            ErrorDict09.Add("30", "	The G53 G-code command requires either a G0 seek or G1 feed motion mode to be active. A different motion was active.");
            ErrorDict09.Add("31", "	There are unused axis words in the block and G80 motion mode cancel is active.");
            ErrorDict09.Add("32", "	A G2 or G3 arc was commanded but there are no XYZ axis words in the selected plane to trace the arc.");
            ErrorDict09.Add("33", "	The motion command has an invalid target. G2, G3, and G38.2 generates this error. For both probing and arcs traced with the radius definition, the current position cannot be the same as the target. This also errors when the arc is mathematically impossible to trace, where the current position, the target position, and the radius of the arc doesn't define a valid arc.");
            ErrorDict09.Add("34", "	A G2 or G3 arc, traced with the radius definition, had a mathematical error when computing the arc geometry. Try either breaking up the arc into semi-circles or quadrants, or redefine them with the arc offset definition.");
            ErrorDict09.Add("35", "	A G2 or G3 arc, traced with the offset definition, is missing the IJK offset word in the selected plane to trace the arc.");
            ErrorDict09.Add("36", "	There are unused, leftover G-code words that aren't used by any command in the block.");
            ErrorDict09.Add("37", "	The G43.1 dynamic tool length offset command cannot apply an offset to an axis other than its configured axis. The Grbl default axis is the Z-axis.");

            ErrorDict11.Add("1", "G-code words consist of a letter and a value. Letter was not found.");
            ErrorDict11.Add("2", "Numeric value format is not valid or missing an expected value.");
            ErrorDict11.Add("3", "Grbl '$' system command was not recognized or supported.");
            ErrorDict11.Add("4", "Negative value received for an expected positive value.");
            ErrorDict11.Add("5", "Homing cycle is not enabled via settings.");
            ErrorDict11.Add("6", "Minimum step pulse time must be greater than 3usec");
            ErrorDict11.Add("7", "EEPROM read failed. Reset and restored to default values.");
            ErrorDict11.Add("8", "Grbl '$' command cannot be used unless Grbl is IDLE. Ensures smooth operation during a job.");
            ErrorDict11.Add("9", "G-code locked out during alarm or jog state");
            ErrorDict11.Add("10", "Soft limits cannot be enabled without homing also enabled.");
            ErrorDict11.Add("11", "Max characters per line exceeded. Line was not processed and executed.");
            ErrorDict11.Add("12", "(Compile Option) Grbl '$' setting value exceeds the maximum step rate supported.");
            ErrorDict11.Add("13", "Safety door detected as opened and door state initiated.");
            ErrorDict11.Add("14", "(Grbl-Mega Only) Build info or startup line exceeded EEPROM line length limit.");
            ErrorDict11.Add("15", "Jog target exceeds machine travel. Command ignored.");
            ErrorDict11.Add("16", "Jog command with no '=' or contains prohibited g-code.");
            ErrorDict11.Add("17", "Laser mode requires PWM output.");
            ErrorDict11.Add("20", "Unsupported or invalid g-code command found in block.");
            ErrorDict11.Add("21", "More than one g-code command from same modal group found in block.");
            ErrorDict11.Add("22", "Feed rate has not yet been set or is undefined.");
            ErrorDict11.Add("23", "G-code command in block requires an integer value.");
            ErrorDict11.Add("24", "Two G-code commands that both require the use of the XYZ axis words were detected in the block.");
            ErrorDict11.Add("25", "A G-code word was repeated in the block.");
            ErrorDict11.Add("26", "A G-code command implicitly or explicitly requires XYZ axis words in the block, but none were detected.");
            ErrorDict11.Add("27", "N line number value is not within the valid range of 1 - 9,999,999.");
            ErrorDict11.Add("28", "A G-code command was sent, but is missing some required P or L value words in the line.");
            ErrorDict11.Add("29", "Grbl supports six work coordinate systems G54-G59. G59.1, G59.2, and G59.3 are not supported.");
            ErrorDict11.Add("30", "The G53 G-code command requires either a G0 seek or G1 feed motion mode to be active. A different motion was active.");
            ErrorDict11.Add("31", "There are unused axis words in the block and G80 motion mode cancel is active.");
            ErrorDict11.Add("32", "A G2 or G3 arc was commanded but there are no XYZ axis words in the selected plane to trace the arc.");
            ErrorDict11.Add("33", "The motion command has an invalid target. G2, G3, and G38.2 generates this error, if the arc is impossible to generate or if the probe target is the current position.");
            ErrorDict11.Add("34", "A G2 or G3 arc, traced with the radius definition, had a mathematical error when computing the arc geometry. Try either breaking up the arc into semi-circles or quadrants, or redefine them with the arc offset definition.");
            ErrorDict11.Add("35", "A G2 or G3 arc, traced with the offset definition, is missing the IJK offset word in the selected plane to trace the arc.");
            ErrorDict11.Add("36", "There are unused, leftover G-code words that aren't used by any command in the block.");
            ErrorDict11.Add("37", "The G43.1 dynamic tool length offset command cannot apply an offset to an axis other than its configured axis. The Grbl default axis is the Z-axis.");
            ErrorDict11.Add("38", "Tool number greater than max supported value.");
        }

        /// <summary>
        /// Get the dictionnary of error message: Key - Value: Error code description
        /// </summary>
        public Dictionary<string, string> ErrorDict09 { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Get the dictionnary of error message: Key: IDXX - Value: Error code description
        /// </summary>
        public Dictionary<string, string> ErrorDict11 { get; set; } = new Dictionary<string, string>();
    }
}
