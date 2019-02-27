using GalaSoft.MvvmLight;
using System.Windows.Media;

namespace VDGrbl.Model
{
    public class GrblModel:ObservableObject
    {
        #region private Members
        public enum MachStatus { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep };
        #endregion

        #region public Properties
        public string RXLine { get; set; }
        public string TXLine { get; set; }
        public MachStatus MachinStatus { get; set; }
        public SolidColorBrush MachinStatusColor { get; set; }
        public string VersionGrbl { get; set; }
        public string BuildInfo { get; set; }
        public string PosX { get; set; }
        public string PosY { get; set; }
        public string PosZ { get; set; }
        #endregion
    }
}
