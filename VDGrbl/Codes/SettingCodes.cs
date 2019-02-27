using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace VDGrbl.Codes
{
    public class SettingCodes
    {
        /// <summary>
        /// List of the system settings that we get after sending $$ command.
        /// In Grbl version 1.1 the description is not save in the Arduino.
        /// </summary>
        public SettingCodes()
        {
            SettingDict.Add("$0", "Step pulse, microseconds.");
            SettingDict.Add("$1", "Step idle delay, milliseconds.");
            SettingDict.Add("$2", "Step port invert, mask.");
            SettingDict.Add("$3", "Direction port invert, mask.");
            SettingDict.Add("$4", "Step enable invert, boolean.");
            SettingDict.Add("$5", "Limit pins invert, boolean.");
            SettingDict.Add("$6", "Probe pin invert, boolean.");
            SettingDict.Add("$10", "Status report, mask.");
            SettingDict.Add("$11", "Junction deviation, mm.");
            SettingDict.Add("$12", "Arc tolerance, mm.");
            SettingDict.Add("$13", "Report inches, boolean.");
            SettingDict.Add("$20", "Soft limits, boolean.");
            SettingDict.Add("$21", "Hard limits, boolean.");
            SettingDict.Add("$22", "Homing cycle, boolean.");
            SettingDict.Add("$23", "Homing dir invert, mask.");
            SettingDict.Add("$24", "Homing feed, mm/min.");
            SettingDict.Add("$25", "Homing seek, mm/min.");
            SettingDict.Add("$26", "Homing debounce, milliseconds.");
            SettingDict.Add("$27", "Homing pull-off, mm.");
            SettingDict.Add("$30", "Max spindle speed, RPM.");
            SettingDict.Add("$31", "Min spindle speed, RPM.");
            SettingDict.Add("$32", "Laser mode, boolean.");
            SettingDict.Add("$100", "X steps/mm.");
            SettingDict.Add("$101", "Y steps/mm.");
            SettingDict.Add("$102", "Z steps/mm.");
            SettingDict.Add("$110", "X Max rate, mm/min.");
            SettingDict.Add("$111", "Y Max rate, mm/min.");
            SettingDict.Add("$112", "Z Max rate, mm/min.");
            SettingDict.Add("$120", "X Acceleration, mm/sec^2.");
            SettingDict.Add("$121", "Y Acceleration, mm/sec^2.");
            SettingDict.Add("$122", "Z Acceleration, mm/sec^2.");
            SettingDict.Add("$130", "X Max travel, mm.");
            SettingDict.Add("$131", "Y Max travel, mm.");
            SettingDict.Add("$132", "Z Max travel, mm.");
        }

        private readonly List<string> _settingCode = new List<string> { "$0", "$1", "$2", "$3", "$4", "$5", "$6", "$10", "$11", "$12", "$13", "$20", "$21", "$22", "$23", "$24", "$25", "$26", "$27","$30","$31","$32","$100","$101","$102","$110","$111","$112","$120","$121","$122","$130","$131","$132" };

        private readonly List<string> _settingDescription = new List<string> {
            "Step pulse, microseconds.",
            "Step idle delay, milliseconds.",
            "Step port invert, mask.",
            "Direction port invert, mask.",
            "Step enable invert, boolean.",
            "Limit pins invert, boolean.",
            "Probe pin invert, boolean.",
            "Status report, mask.",
            "Junction deviation, mm.",
            "Arc tolerance, mm.",
            "Report inches, boolean.",
            "Soft limits, boolean.",
            "Hard limits, boolean.",
            "Homing cycle, boolean.",
            "Homing dir invert, mask.",
            "Homing feed, mm/min.",
            "Homing seek, mm/min.",
            "Homing debounce, milliseconds.",
            "Homing pull-off, mm.",
            "Max spindle speed, RPM.",
            "Min spindle speed, RPM.",
            "Laser mode, boolean.",
            "X steps/mm.",
            "Y steps/mm.",
            "Z steps/mm.",
            "X Max rate, mm/min.",
            "Y Max rate, mm/min.",
            "Z Max rate, mm/min.",
            "X Acceleration, mm/sec^2.",
            "Y Acceleration, mm/sec^2.",
            "Z Acceleration, mm/sec^2.",
            "X Max travel, mm.",
            "Y Max travel, mm.",
            "Z Max travel, mm.",
        };

        /// <summary>
        /// Get the dictionnary of setting message: Key: IDXX - Value: Setting code description
        /// </summary>
        public Dictionary<string, string> SettingDict { get; set; } = new Dictionary<string, string>();
    }
}
