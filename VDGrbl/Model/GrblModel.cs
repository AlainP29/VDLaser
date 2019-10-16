namespace VDGrbl.Model
{
    public class GrblModel
    {
        public string GrblCommandHeader { get; private set; } = "Command";
        public string GrblRXData { get; private set; }
        public string GrblTXData { get; private set; }

        public GrblModel(string grblHeader)
        {
            GrblCommandHeader += grblHeader;
        }

        public GrblModel(string txData, string rxData)
        {
            GrblTXData = txData;
            GrblRXData = rxData;
        }

    }
}
