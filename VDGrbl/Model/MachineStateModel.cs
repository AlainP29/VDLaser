namespace VDGrbl.Model
{
    /// <summary>
    /// Coordinate model: add X, Y properties and constructor?
    /// </summary>
    public class MachineStateModel
    {
        public string MachineStateHeader { get; private set; }

        public MachineStateModel(string machineStateHeader)
        {
            MachineStateHeader = machineStateHeader;
        }
    }
}
