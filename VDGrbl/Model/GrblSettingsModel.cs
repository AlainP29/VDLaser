namespace VDGrbl.Model
{
    public class GrblSettingsModel
    {
        public string GrblSettingsHeader { get; private set; }
        public string GrblSettingCode { get; private set; }
        public string GrblSettingValue { get; private set; }
        public string GrblSettingDescription { get; private set; }

        public GrblSettingsModel()
        { }
        public GrblSettingsModel(string grblSettingsHeader)
        {
            GrblSettingsHeader = grblSettingsHeader;
        }
        public GrblSettingsModel(string settingCode, string settingValue, string settingDescription)
        {
            GrblSettingCode = settingCode;
            GrblSettingValue = settingValue;
            GrblSettingDescription = settingDescription;
        }
    }
}
