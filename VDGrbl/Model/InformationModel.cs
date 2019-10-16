namespace VDGrbl.Model
{
    public class InformationModel
    {
        public string InformationHeader { get; private set; }
        public string Version { get; private set; }
        public string Build { get; private set; }

        public InformationModel(string informationHeader)
        {
            InformationHeader = informationHeader;
        }

    }
}
