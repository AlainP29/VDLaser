namespace VDGrbl.Model
{
    public class DataFieldModel
    {
        public string DataFieldHeader { get; private set; }
        public double MposX { get; private set; }
        public double MposY { get; private set; }
        public double WposX { get; private set; }
        public double WposY { get; private set; }
        public double WCO { get; private set; }
        public double Feed { get; private set; }
        public double SpindleSpeed { get; private set; }
        public int BufferState { get; private set; }

        public MachineStatus MachineState { get; private set; }

        public enum MachineStatus { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep };

        public DataFieldModel()
        { }
        public DataFieldModel(string dataFieldHeader)
        {
            DataFieldHeader = dataFieldHeader;
        }
    }
}
