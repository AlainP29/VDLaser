using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Models;

namespace VDLaser.Core.Gcode.Models
{
    public class GcodeState
    {
        /// <summary>
        /// Enumeration of the response states. Ok: All is good, NOk: Alarm state Q: Queued [DR: Data received] 
        /// </summary>
        public enum RespStatus { Ok, NOk, Q };
        public RespStatus GCodeStatus { get; set; } = RespStatus.Ok;
        /// <summary> Dernière erreur GCode reçue (error:#). </summary>
        public GrblError? Error { get; set; }
        public bool IsGrbl11 { get; set; } = false;

        /// <summary> Historique de toutes les erreurs reçues. </summary>
        public List<GrblError> ErrorHistory { get; } = new();
        /// <summary> Message d’erreur si un parsing échoue. </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        /// <summary>
        /// Permet de réinitialiser l’état GCode (erreur seulement mais pas les paramètres et alarme => GrblState).
        /// </summary>
        public void ResetStatus()
        {
            Error = null;
            ErrorMessage = string.Empty;
            GCodeStatus = RespStatus.Ok;
        }
    }
}
