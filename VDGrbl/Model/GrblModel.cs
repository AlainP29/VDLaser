using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGrbl.Model
{
    class GrblModel
    {
        #region private Members
        public enum Status { Disconnected, Connecting, Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep };
        #endregion

        #region public Properties
        public string RXLine { get; private set; }
        public string TXLine { get; private set; }
        public Status MachineStatus {get; private set;}//dans view model car une seule tram reçue...?
        public string Coordinate { get; private set; }
        #endregion

        #region Constructors
        #endregion
    }
}
