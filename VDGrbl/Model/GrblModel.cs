namespace VDGrbl.Model
{
    public class GrblModel
    {
        public string GrblConsoleHeader { get; private set; } = "Data console";
        public string GrblControlHeader { get; private set; } = "Machine Control";
        public string GrblCommandHeader { get; private set; } = "Command";
        public string GrblRXData { get; private set; }
        public string GrblTXData { get; private set; }

        public GrblModel()
        { }
        public GrblModel(string grblHeader)
        {
            GrblConsoleHeader += grblHeader;
            GrblCommandHeader += grblHeader;
            GrblControlHeader += grblHeader;
        }

        public GrblModel(string txData, string rxData)
        {
            GrblTXData = txData;
            GrblRXData = rxData;
        }

    }
}
