using System.Collections.Generic;

namespace VDGrbl.Codes
{
    class AlarmCodes
    {
        /// <summary>
        /// Description of the alarm messages of Grbl version 0.9 and 1.1
        /// </summary>
        public AlarmCodes()
        {
            AlarmDict11.Add("1", "Hard limit triggered. Machine position is likely lost due to sudden and immediate halt. Re-homing is highly recommended.");
            AlarmDict11.Add("2", "G-code motion target exceeds machine travel. Machine position safely retained. Alarm may be unlocked.");
            AlarmDict11.Add("3", "Reset while in motion. Grbl cannot guarantee position. Lost steps are likely. Re-homing is highly recommended.");
            AlarmDict11.Add("4", "Probe fail. The probe is not in the expected initial state before starting probe cycle, where G38.2 and G38.3 is not triggered and G38.4 and G38.5 is triggered.");
            AlarmDict11.Add("5", "Probe fail. Probe did not contact the workpiece within the programmed travel for G38.2 and G38.4.");
            AlarmDict11.Add("6", "Homing fail. Reset during active homing cycle.");
            AlarmDict11.Add("7", "Homing fail. Safety door was opened during active homing cycle.");
            AlarmDict11.Add("8", "Homing fail. Cycle failed to clear limit switch when pulling off. Try increasing pull-off setting or check wiring.");
            AlarmDict11.Add("9", "Homing fail. Could not find limit switch within search distance. Defined as 1.5 * max_travel on search and 5 * pulloff on locate phases.");

            AlarmDict09.Add("Hard / soft limit", "Hard and / or soft limits must be enabled for this error to occur.With hard limits, Grbl will enter alarm mode when a hard limit switch has been triggered and force kills all motion.Machine position will be lost and require re-homing.With soft limits, the alarm occurs when Grbl detects a programmed motion trying to move outside of the machine space, set by homing and the max travel settings.However, upon the alarm, a soft limit violation will instruct a feed hold and wait until the machine has stopped before issuing the alarm. Soft limits do not lose machine position because of this.");
            AlarmDict09.Add("Abort during cycle","This alarm occurs when a user issues a soft-reset while the machine is in a cycle and moving. The soft-reset will kill all current motion, and, much like the hard limit alarm, the uncontrolled stop causes Grbl to lose position.");
            AlarmDict09.Add("Probe fail","The G38.2 straight probe command requires an alarm or error when the probe fails to trigger within the programmed probe distance. Grbl enters the alarm state to indicate to the user the probe has failed, but will not lose machine position, since the probe motion comes to a controlled stop before the error.");
        }
        /// <summary>
        /// Get the dictionnary of alarm messages version 1.1: Key: IDXX - Value: Alarm code description
        /// </summary>
        public Dictionary<string, string> AlarmDict11 { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Get the dictionnary of alarm messages version 0.9: Key - Value: Alarm code description
        /// </summary>
        public Dictionary<string, string> AlarmDict09 { get; set; } = new Dictionary<string, string>();
    }
}

