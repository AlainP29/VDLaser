namespace VDGrbl.Model
{
    public class ControlModel
    {
        public string ControlHeader { get; private set; }

        public ControlModel(string controlHeader)
        {
            ControlHeader = controlHeader;
        }
    }
}
