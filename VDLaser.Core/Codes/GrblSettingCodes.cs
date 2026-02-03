using VDLaser.Core.Grbl.Models;

namespace VDLaser.Core.Codes
{
    public class GrblSettingCodes
    {
        public Dictionary<int, GrblSetting> Settings09 { get; }
        public Dictionary<int, GrblSetting> Settings11 { get; }

        public GrblSettingCodes()
        {
            Settings09 = BuildGrbl09();
            Settings11 = BuildGrbl11();
        }

        /// <summary>
        /// GET SETTING BY VERSION
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isVersion11"></param>
        /// <returns></returns>
        public GrblSetting? GetSetting(int id, bool isVersion11)
        {
            var dict = isVersion11 ? Settings11 : Settings09;
            return dict.TryGetValue(id, out var s) ? s : null;
        }

        /// <summary>
        /// GRBL 0.9 DEFINITIONS
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, GrblSetting> BuildGrbl09()
        {
            return new Dictionary<int, GrblSetting>
            {
                {0,  new GrblSetting(0, "$0", "Step pulse time", "µs")},
                {1,  new GrblSetting(1, "$1", "Step idle delay", "ms")},
                {2,  new GrblSetting(2, "$2", "Step port invert mask","")},
                {3,  new GrblSetting(3, "$3", "Direction port invert mask", "")},
                {4,  new GrblSetting(4, "$4", "Step enable invert", "bool")},
                {5,  new GrblSetting(5, "$5", "Limit pins invert", "bool")},
                {6,  new GrblSetting(6, "$6", "Probe pin invert", "bool")},

                {10, new GrblSetting(10, "$10", "Status report mask", "")},
                {11, new GrblSetting(11, "$11", "Junction deviation", "mm")},
                {12, new GrblSetting(12, "$12", "Arc tolerance", "mm")},
                {13, new GrblSetting(13, "$13", "Report inches", "bool")},

                {20, new GrblSetting(20, "$20", "Soft limits enable", "bool")},
                {21, new GrblSetting(21, "$21", "Hard limits enable", "bool")},
                {22, new GrblSetting(22, "$22", "Homing enable", "bool")},
                {23, new GrblSetting(23, "$23", "Homing direction mask", "")},
                {24, new GrblSetting(24, "$24", "Homing feed", "mm/min")},
                {25, new GrblSetting(25, "$25", "Homing seek", "mm/min")},
                {26, new GrblSetting(26, "$26", "Homing debounce delay", "ms")},
                {27, new GrblSetting(27, "$27", "Homing pull-off", "mm")},

                {30, new GrblSetting(30, "$30", "Max spindle speed", "RPM")},
                {31, new GrblSetting(31, "$31", "Min spindle speed", "RPM")},
                {32, new GrblSetting(32, "$32", "Laser mode", "bool")},

                {100, new GrblSetting(100, "$100", "X steps/mm", "")},
                {101, new GrblSetting(101, "$101", "Y steps/mm", "")},
                {102, new GrblSetting(102, "$102", "Z steps/mm", "")},

                {110, new GrblSetting(110, "$110", "X max rate", "mm/min")},
                {111, new GrblSetting(111, "$111", "Y max rate", "mm/min")},
                {112, new GrblSetting(112, "$112", "Z max rate", "mm/min")},

                {120, new GrblSetting(120, "$120", "X acceleration", "mm/s²")},
                {121, new GrblSetting(121, "$121", "Y acceleration", "mm/s²")},
                {122, new GrblSetting(122, "$122", "Z acceleration", "mm/s²")},

                {130, new GrblSetting(130, "$130", "X max travel", "mm")},
                {131, new GrblSetting(131, "$131", "Y max travel", "mm")},
                {132, new GrblSetting(132, "$132", "Z max travel", "mm")},
            };
        }

        /// <summary>
        /// GRBL 1.1 DEFINITIONS
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, GrblSetting> BuildGrbl11()
        {
            var dict = BuildGrbl09(); // identical base
            // Version 1.1 adds:
            dict[140] = new GrblSetting(140, "$140", "X jerk limit", "mm/min³");
            dict[141] = new GrblSetting(141, "$141", "Y jerk limit", "mm/min³");
            dict[142] = new GrblSetting(142, "$142", "Z jerk limit", "mm/min³");

            return dict;
        }
    }
}

