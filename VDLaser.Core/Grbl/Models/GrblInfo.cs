using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Grbl.Models
{
    public class GrblInfo
    {
        // <summary>
        /// Version GRBL ("Grbl 1.1h ['$' for help]").
        /// </summary>
        public string GrblVersion { get; set; } = string.Empty;
        /// <summary>
        /// Build GRBL [VER:1.1d.20161014:].
        /// </summary>
        public string GrblBuild { get; set; } = string.Empty;

        /// <summary>
        /// Ligne affichée au démarrage par GRBL: Grbl X.Xx ['$' for help]
        /// </summary>
        public string WelcomMessage { get; set; } = string.Empty;

        /// <summary>
        /// Feedback messages provide non-critical information []:MSG,GC,HLP
        /// </summary>
        public string FeedbackMessage { get; set; } = string.Empty;

        /// <summary>
        /// Codes for compile-time options enabled in the firmware. [OPT:VL,15,128]
        /// </summary>
        public string CompileOptions { get; set; } = string.Empty;
        public string BlockBufferSize { get; set; } = string.Empty;
        public string RxBufferSize { get; set; } = string.Empty;
        public string HelpMessage { get; set; } = string.Empty;
        public string GCodeMessage { get; set; } = string.Empty;
    }
}
