using System.Windows.Media;  // Pour SolidColorBrush si besoin

namespace VDLaser.Core.Models
{
    public partial class GrblItems
    {
        public enum MachState { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep, Undefined };

        public double MPosX { get; private set; } = 0;

        public double MPosY { get; private set; } = 0;

        public double WPosX { get; private set; } = 0;

        public double WPosY { get; private set; } = 0;

        public int FeedRate { get; private set; } = 0;

        public string VersionGrbl { get; private set; } = "0.0";

        public string AlarmMessage { get; private set; } = "";

        public int LaserPower { get; private set; } = 0;

        public int BufferRate { get; private set; } = 0;

        public string MachineStateHeader { get; private set; } = "0";

        public string ControleHeader { get; private set; } = "0";

        public MachState MachineState = MachState.Undefined;

        public SolidColorBrush MachStateColor = Brushes.DarkGray;

    }
}