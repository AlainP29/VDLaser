namespace VDGrbl.Model
{
    /// <summary>
    /// Grbl model class.
    /// </summary>
    public class GrblModel
    {
        public string GrblConsoleHeader { get; private set; } = "Data console";
        public string GrblControlHeader { get; private set; } = "Machine Control";
        public string GrblSettingHeader { get; private set; } = "Setting";
        public string GrblCommandHeader { get; private set; } = "Command";
        public string GrblRXData { get; private set; }
        public string GrblTXData { get; private set; }
        public string GrblSettingCode { get; private set; }
        public string GrblSettingValue { get; private set; }
        public string GrblSettingDescription { get; private set; }

        public GrblModel(string grblHeader)
        {
            GrblConsoleHeader += grblHeader;
            GrblSettingHeader += grblHeader;
            GrblCommandHeader += grblHeader;
            GrblControlHeader += grblHeader;
        }

        public GrblModel(string txData, string rxData)
        {
            GrblTXData = txData;
            GrblRXData = rxData;
        }

        public GrblModel(string settingCode, string settingValue, string settingDescription)
        {
            GrblSettingCode = settingCode;
            GrblSettingValue = settingValue;
            GrblSettingDescription = settingDescription;
        }
    }
}
